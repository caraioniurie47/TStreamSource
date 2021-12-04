#region LICENSE

//  TStreamSource - MPEG Stream Tools
//  Copyright (C) 2011 Iurie Caraion (caraioniurie47@gmail.com)
//  https://github.com/caraioniurie47/TStreamSource

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
    using System.IO;
    using System.Runtime.InteropServices;

    using NLog;

    #endregion

    /// <summary>
    /// Implementation of File stream source.
    /// </summary>
    public sealed class FileSource : AbstractSource, IStreamSource
    {
        #region Logger

        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion

        #region Fields

        /// <summary>
        /// The path to file.
        /// </summary>
        private readonly string filename;

        /// <summary>
        /// The filestream.
        /// </summary>
        private FileStream fileStream;

        #endregion

        #region Constructors

        /// <summary>
        /// The Constructor.
        /// </summary>
        /// <param name="filename">Path to file.</param>
        public FileSource(string filename)
        {
            Ensure.IsNotNullOrEmpty(Log, filename, "filename is null or empty");
            this.filename = filename;
        }

        #endregion

        #region Methods

        #region Open Method

        /// <summary>
        /// Open file for reading.
        /// </summary>
        protected override void Open()
        {
            this.CheckDisposed();
            
            if (!File.Exists(this.filename))
            {
                throw new CustomException("File not found");    
            }

            this.fileStream = File.Open(this.filename, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        #endregion

        #region Close Method

        /// <summary>
        /// Close file stream.
        /// </summary>
        protected override void Close()
        {
            this.CheckDisposed();

            if (this.fileStream == null)
            {
                return;
            }

            this.fileStream.Dispose();
            this.fileStream = null;
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

            var len = count * PacketLength;
            var bytes = new byte[len];
            var read = this.fileStream.Read(bytes, 0, len);
            if (read == len)
            {
                return new ArraySegment<byte>(bytes);
            }
            
            return null;
        }

        /// <summary>
        /// Reads a number of packets into buffer.
        /// </summary>
        /// <param name="buffer">Pointer to the buffer.</param>
        /// <param name="count">The number of packets to read.</param>
        /// <returns>The total number of bytes read into the buffer. or zero (0) if the end of the stream has been reached.</returns>
        public int Read(IntPtr buffer, int count)
        {
            this.CheckDisposed();
            this.CheckNonConfigured();

            var len = count * PacketLength;
            var bytes = new byte[len];
            var read = this.fileStream.Read(bytes, 0, len);
            if (read != 0)
            {
                Marshal.Copy(bytes, 0, buffer, read);
            }

            return read;
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
            this.CheckDisposed();
            this.CheckNonConfigured();

            var len = count * PacketLength;
            var remaining = this.fileStream.Length - this.fileStream.Position;
            return len <= remaining;
        }

        #endregion

        #region Reset

        /// <summary>
        /// Reset stream to start.
        /// </summary>
        public void Reset()
        {
            this.fileStream.Seek(0, SeekOrigin.Begin);
        }

        #endregion

        #endregion
    }
}
