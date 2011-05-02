#region LICENSE

//  TStreamSource - MPEG Stream Tools
//  Copyright (C) 2011 MarkTwen (mktwen@gmail.com)
//  http://www.TStreamSource.com

//  This library is free software; you can redistribute it and/or
//  modify it under the terms of the GNU Lesser General Public
//  License as published by the Free Software Foundation; either
//  version 3 of the License, or (at your option) any later version.

//  This library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//  Lesser General Public License for more details.

//  You should have received a copy of the GNU Lesser General
//  Public License along with this library; 
//  If not, see <http://www.gnu.org/licenses/>.

#endregion

namespace TStreamSource.Core.Sources
{
    #region Usings

    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Threading;

    using NLog;

    #endregion

    /// <summary>
    /// Implementation of UDP stream source.
    /// TODO: add no signal check
    /// </summary>
    public sealed class UdpSource : AbstractSource, IStreamSource
    {
        #region Logger

        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion

        #region Fields

        /// <summary>
        /// The backing field for source address.
        /// </summary>
        private readonly IPEndPoint ipEndPoint;

        /// <summary>
        /// If true will abort the reading thread.
        /// </summary>
        private volatile bool abortThread;
        
        /// <summary>
        /// The reading thread.
        /// </summary>
        private Thread readThread;

        /// <summary>
        /// The socket.
        /// </summary>
        private Socket theSocket;
        
        /// <summary>
        /// The buffer.
        /// </summary>
        private byte[] memory;

        /// <summary>
        /// The write index.
        /// </summary>
        private long writeIndex;

        /// <summary>
        /// The read index.
        /// </summary>
        private long readIndex;

        /// <summary>
        /// The buffer size.
        /// </summary>
        private int bufferSize;

        /// <summary>
        /// The locker.
        /// </summary>
        private readonly object locker = new object();

        #endregion

        #region Constructors

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="address">IP address of source.</param>
        /// <param name="port">The port.</param>
        public UdpSource(string address, int port)
        {
            if (string.IsNullOrEmpty(address))
            {
                address = "127.0.0.1";
            }

            ipEndPoint = new IPEndPoint(IPAddress.Parse(address), port);
        }

        #endregion

        #region Methods

        #region Open Method

        /// <summary>
        /// Open an UDP connection and starts reading thread.
        /// </summary>
        protected override void Open()
        {
            this.CheckDisposed();

            //GCSettings.LatencyMode = GCLatencyMode.LowLatency;

            this.bufferSize = 7 * Constants.TSPacketSize * Constants.DShowNumberOfPacketsTS * 1000;
            this.memory = new byte[this.bufferSize];
            this.writeIndex = 0;
            this.readIndex = 0;

            var onlyPort = new IPEndPoint(IPAddress.Any, ipEndPoint.Port);
            this.theSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.theSocket.ReceiveBufferSize = 65536;

            if (IsMulticastAddress(ipEndPoint))
            {
                this.theSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            }

            this.theSocket.Bind(onlyPort);

            if (IsMulticastAddress(ipEndPoint))
            {    
                this.theSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
                    new MulticastOption(ipEndPoint.Address));
            }

            readThread = new Thread(this.ReadThread);
            readThread.Start();

            Thread.Sleep(700);
            //Thread.Sleep(7000);
        }

        #endregion

        #region Close Method

        /// <summary>
        /// Stop reading thread and close UDP connection.
        /// </summary>
        protected override void Close()
        {
            this.CheckDisposed();

            //GCSettings.LatencyMode = GCLatencyMode.Interactive;

            this.memory = null;
            this.writeIndex = 0;
            this.readIndex = 0;

            abortThread = true;
            if (theSocket != null)
            {
                theSocket.Close();
                theSocket = null;
            }

            readThread.Join();
            readThread = null;
            abortThread = false;
        }

        #endregion

        #region Read Methods

        /// <summary>
        /// Read from stream one or few packets.
        /// </summary>
        /// <returns>The array segment of found bytes, or null if no packets are found.</returns>
        public ArraySegment<byte>? Read(int count)
        {
            this.CheckDisposed();
            this.CheckNonConfigured();

            if (!Require(count))
            {
                return null;
            }

            try
            {
                var len = count * PacketLength;
                var index = Convert.ToInt32(this.readIndex % this.bufferSize);
                var result = new ArraySegment<byte>(this.memory, index, len);
                
                this.readIndex += len;

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Reads a number of packets into buffer.
        /// </summary>
        /// <param name="buffer">Pointer to the buffer.</param>
        /// <param name="count">The number of packets to read.</param>
        /// <returns>The total number of bytes read into the buffer. or zero (0) if the end of the stream has been reached.</returns>
        public int Read(IntPtr buffer, int count)
        {
            //this.CheckDisposed();
            //this.CheckNonConfigured();

            if (this.memory == null)
            {
                return 0;
            }

            if (!Require(count))
            {
                return 0;
            }

            try
            {
                var len = count * PacketLength;
                var index = Convert.ToInt32(this.readIndex % this.bufferSize);
                Marshal.Copy(this.memory, index, buffer, len);    

                //if (this.readIndex + len <= this.bufferSize)
                //{
                    
                //}
                //else
                //{
                //    var nLen = this.readIndex + len - this.bufferSize;
                //    Marshal.Copy(this.memory, this.readIndex, buffer, len);    
                //}

                this.readIndex += len;
                return len;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region Require

        /// <summary>
        /// Block calling thread until the source has requested number of packets.
        /// </summary>
        /// <param name="count">The required number of packets.</param>
        /// <returns>true if number of requested packets are available, otherwise false.</returns>
        public bool Require(int count)
        {
            //this.CheckDisposed();
            //this.CheckNonConfigured();

            var len = count * PacketLength;
            
            lock (locker)
            {
                var index = Interlocked.CompareExchange(ref writeIndex, 0, 0);
                var remaining = index - readIndex;

                while (!(remaining >= 0 && len <= remaining))
                {
                    if (!Monitor.Wait(locker, 1000))
                    {
                        return false;
                    }

                    index = Interlocked.CompareExchange(ref writeIndex, 0, 0);
                    remaining = index - readIndex;
                }
            }

            return true;
        }

        #endregion

        #region Reset

        /// <summary>
        /// Reset stream to start.
        /// </summary>
        public void Reset()
        {
            this.readIndex = 0;
        }

        #endregion

        #endregion

        #region Helper Methods

        #region IsMulticastAddress

        /// <summary>
        /// Checks if ip address is of multicast type.
        /// </summary>
        /// <param name="endPoint">The endpoint which contains IP address.</param>
        /// <returns>true if the address is multicast, otherwise returns false.</returns>
        private static bool IsMulticastAddress(IPEndPoint endPoint)
        {
            Ensure.IsNotNull(Log, endPoint, "endPoint is null");
            var bytes = endPoint.Address.GetAddressBytes();
            return bytes[0] >= 224 && bytes[0] <= 239;
        }

        #endregion

        #region ReadThread

        /// <summary>
        /// The Reading Thread.
        /// </summary>
        private void ReadThread()
        {
            while (!abortThread)
            {
                try
                {
                    var indexLong = Interlocked.CompareExchange(ref writeIndex, 0, 0);
                    var index = Convert.ToInt32(indexLong % bufferSize);

                    var len = theSocket.Receive(this.memory, index, 7 * Constants.TSPacketSize, SocketFlags.None);

                    var ok = true;
                    for (var i = index; i < index + len; i += Constants.TSPacketSize)
                    {
                        if (this.memory[i] != 0x47)
                        {
                            ok = false;
                            break;
                        }
                    }

                    if (ok)
                    {
                        Interlocked.Add(ref writeIndex, len);

                        lock (locker)
                        {
                            Monitor.Pulse(locker);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorException("ReadThread throws an exception", ex);
                    return;
                }
            }
        }

        #endregion

        #endregion
    }
}
