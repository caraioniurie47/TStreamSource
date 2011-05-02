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

namespace TStreamSource.Core.ObjectPools
{
    #region Usings

    using System;
    using NLog;

    #endregion

    public sealed class ArrayBlockPoolDummy : IArrayBlockPool
    {
        #region Logger

        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion

        #region Fields

        private readonly int bufferLength;

        #endregion

        #region Properties

        #region BlockSize

        /// <summary>
        /// Size of the block.
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
        /// Initializes a new instance of the <see cref="ArrayBlockPoolDummy"/> class.
        /// </summary>
        /// <param name="bufferLength">Length of the buffer.</param>
        public ArrayBlockPoolDummy(int bufferLength)
        {
            Ensure.GreaterZero(Log, bufferLength, "buffer length <= 0");
            this.bufferLength = bufferLength;
        }

        #endregion

        #region Methods

        #region Allocate Methods

        /// <summary>
        /// The block is retrieved from object pool.
        /// </summary>
        /// <returns>The array block.</returns>
        public IArrayBlock Allocate()
        {
            this.CheckDisposed();
            LeakStatus++;
            return new ArrayBlock(bufferLength);
        }

        public void Release(IArrayBlock item)
        {
            LeakStatus--;
            this.CheckDisposed();
        }

        #endregion

        #endregion

        #region IDisposable Members

        private bool disposed;

        #region Finalizer

        /// <summary>
        /// The Finalizer.
        /// </summary>
        ~ArrayBlockPoolDummy()
        {
            Dispose(false);
        }

        #endregion

        #region Dispose Methods

        /// <summary>
        /// Dispose method.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.disposed) return;

            if (disposing)
            {
                // TODO
            }

            this.disposed = true;
        }

        #endregion

        #region CheckDisposed

        /// <summary>
        /// Check if object is disposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(GetType().ToString());
            }
        }

        #endregion

        #endregion
    }
}
