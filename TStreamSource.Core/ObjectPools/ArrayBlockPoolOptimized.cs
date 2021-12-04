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

namespace TStreamSource.Core.ObjectPools
{
    #region Usings

    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    using NLog;

    #endregion

    /// <summary>
    /// Pools data buffers to prevent both frequent allocation and memory fragmentation
    /// due to pinning in high volume scenarios.
    /// See https://blogs.msdn.com/yunjin/archive/2004/01/27/63642.aspx
    /// </summary>
    public sealed class ArrayBlockPoolOptimized : IArrayBlockPool
    {
        #region Logger

        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion

        #region Fields

        private byte[] buffer;
        private int bufferLength;
        private IArrayBlock[] blocks;
        private Stack<int> freeIndexPool;
        private int totalBytes;
        
        private int currentIndex;

        private GCHandle handleToArray;

        #endregion

        #region Properties

        #region Available

        /// <summary>
        /// The number of available objects in the pool.
        /// </summary>
        public int Available
        {
            get
            {
                CheckDisposed();
                lock (this.freeIndexPool)
                {
                    return ((this.totalBytes - this.currentIndex) / this.bufferLength) + this.freeIndexPool.Count;
                }
            }
        }

        #endregion

        #region Indexer

        public IArrayBlock this[int offset]
        {
            get
            {
                CheckDisposed();
                return this.blocks[offset / this.bufferLength];
            }
        }

        #endregion

        #region BlockSize

        /// <summary>
        /// Size of the minimal block.
        /// </summary>
        public int BlockSize
        {
            get { return this.bufferLength; }
        }

        #endregion

        #region LeakStatus

        /// <summary>
        /// Shows number of leaked objects.
        /// </summary>
        public int LeakStatus { get; private set; }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Pools data buffers to prevent both frequent allocation and memory fragmentation
        /// due to pinning in high volume scenarios.
        /// </summary>
        /// <param name="numberOfBuffers">The total number of buffers that will be allocated.</param>
        /// <param name="bufferLength">The size of each buffer.</param>
        public ArrayBlockPoolOptimized(int numberOfBuffers, int bufferLength)
        {
            Ensure.GreaterZero(Log, numberOfBuffers, "number of buffers <= 0");
            Ensure.GreaterZero(Log, bufferLength, "buffer length <= 0");

            this.bufferLength = bufferLength;
            this.totalBytes = numberOfBuffers * this.bufferLength;
            this.freeIndexPool = new Stack<int>();
            this.buffer = new byte[this.totalBytes];
            this.blocks = new IArrayBlock[numberOfBuffers];

            for (var i = 0; i < numberOfBuffers; i++)
            {
                this.blocks[i] = new ArrayBlock(this, i, this.buffer, bufferLength);
            }

            this.handleToArray = GCHandle.Alloc(this.buffer, GCHandleType.Pinned);
        }

        #endregion

        #region Allocate Methods

        /// <summary>
        /// The block is retrieved from object pool.
        /// </summary>
        /// <returns>The array block.</returns>
        public IArrayBlock Allocate()
        {
            CheckDisposed();

            var idx = CheckOutIdx();
            if (idx >= this.blocks.Length)
            {
                try
                {
                    LeakStatus++;
                    return new ArrayBlock(bufferLength);
                }
                catch(Exception ex)
                {
                    Log.ErrorException("Error in Object Pool", ex);
                    throw;
                }
            }

            LeakStatus++;
            return this.blocks[idx];
        }

        #endregion

        #region Release Method

        public void Release(IArrayBlock item)
        {
            CheckDisposed();

            LeakStatus--;
            var block = Ensure.IsNotNull(Log, item as ArrayBlock, "item is null or is not ArrayBlock");
            var id = block.Id;
            CheckIn(id);
        }

        #endregion

        #region Helper Methods

        private void CheckIn(int idx)
        {
            lock (this.freeIndexPool)
            {
                if (!this.freeIndexPool.Contains(idx))
                {
                    this.freeIndexPool.Push(idx);
                }
                else
                {
                    Log.Fatal("Multiple releases for {0}", idx);
                }
            }
        }

        private int CheckOutIdx()
        {
            int idx;

            lock (this.freeIndexPool)
            {
                if (this.freeIndexPool.Count > 0)
                {
                    idx = this.freeIndexPool.Pop();
                }
                else
                {
                    idx = this.currentIndex;
                    this.currentIndex++;
                }
            }

            return idx;
        }

        #endregion

        #region IDisposable Members

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ArrayBlockPoolOptimized()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (this.disposed) return;

            if (disposing)
            {
                if (LeakStatus != 0)
                {
                    Log.Warn("Object pool leak, Number of leaked objects {0}", LeakStatus);
                }

                freeIndexPool.Clear();

                foreach (var block in this.blocks)
                {
                    block.Release();
                }

                freeIndexPool = null;

                buffer = null;
                bufferLength = 0;
                blocks = null;
                totalBytes = 0;
                currentIndex = 0;

                if (this.handleToArray.IsAllocated)
                {
                    this.handleToArray.Free();
                }
            }

            this.disposed = true;
        }

        private void CheckDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(GetType().ToString());
            }
        }

        #endregion
    }
}