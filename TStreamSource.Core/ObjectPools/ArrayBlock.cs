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
    using System.Runtime.InteropServices;

    using NLog;

    #endregion
    
    /// <summary>
    /// ArrayBlock implementation.
    /// </summary>
    public sealed class ArrayBlock : IArrayBlock
    {
        #region Logger

        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion

        #region Fields

        /// <summary>
        /// The array block pool instance.
        /// </summary>
        private readonly IArrayBlockPool arrayBlockPool;

        #endregion

        #region Properties

        #region Id

        private readonly int id;

        /// <summary>
        /// Identification for current block.
        /// </summary>
        public int Id
        {
            get { return this.id; }
        }

        #endregion

        #region Buffer

        private readonly byte[] buffer;

        /// <summary>
        /// The buffer that is used by this block.
        /// </summary>
        /// <value>The buffer.</value>
        public byte[] Buffer
        {
            get { return this.buffer; }
        }

        #endregion

        #region Length

        private readonly int length;

        /// <summary>
        /// The maximum amount of block data, in bytes, to write or read from the buffer.
        /// </summary>
        public int Length
        {
            get { return this.length; }
        }

        #endregion

        #region Offset

        private readonly int offset;

        /// <summary>
        /// The offset, in bytes, in the buffer where the block data starts.
        /// </summary>
        public int Offset
        {
            get { return this.offset; }
        }

        #endregion

        #region PtrToData

        private readonly IntPtr ptrToData;

        /// <summary>
        /// Pointer to data contained in the block.
        /// </summary>
        public IntPtr PtrToData
        {
            get { return ptrToData; }
        }

        #endregion

        #endregion

        #region Indexer

        /// <summary>
        /// Offset-based indexer for byte read/write. Hidden regions are not set or get.
        /// </summary>
        /// <param name="idx">Index, it starts with zero and is relative to offset.</param>
        /// <returns>Byte value from idx position in buffer relative to the offset.</returns>
        public byte this[int idx]
        {
            get
            {
                this.EnsureIndex(idx);
                return this.buffer[Offset + idx];
            }

            set
            {
                this.EnsureIndex(idx);
                this.buffer[Offset + idx] = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayBlock"/> class.
        /// </summary>
        /// <param name="length">The length.</param>
        public ArrayBlock(int length)
            : this(null, 0, new byte[length], length)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayBlock"/> class.
        /// </summary>
        /// <param name="arrayBlockPool">The array block pool.</param>
        /// <param name="id">The Id.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="length">The length.</param>
        public ArrayBlock(IArrayBlockPool arrayBlockPool, int id, byte[] buffer, int length)
        {
            Ensure.GreaterOrEqualZero(Log, id, "Id is negative");
            Ensure.IsNotNull(Log, buffer, "buffer is null");
            Ensure.GreaterZero(Log, length, "length <= 0");
            Ensure.Greater(Log, buffer.Length, (id + 1) * length - 1, "length must be less than maximum offset");
            Ensure.IsTrue(Log, buffer.Length % length == 0, "buffer length must be divided by length");

            this.arrayBlockPool = arrayBlockPool;

            this.id = id;
            this.buffer = buffer;

            this.length = length;
            this.offset = id * length;

            // buffer must be pinned.
            this.ptrToData = this.arrayBlockPool != null ? Marshal.UnsafeAddrOfPinnedArrayElement(buffer, this.offset) : IntPtr.Zero;
        }

        #endregion

        #region Fill Methods

        /// <summary>
        /// Fill the entire block's internal buffer with zero value.
        /// </summary>
        public void Zero()
        {
            this.Fill(0);
        }

        /// <summary>
        /// Fill the entire block's internal buffer with <paramref name="val"/> value.
        /// </summary>
        /// <param name="val">The value.</param>
        public void Fill(byte val)
        {
            for (var i = 0; i < this.Length; i++)
            {
                this[i] = val;
            }
        }

        #endregion

        #region IEquatable Implementation

        /// <summary>
        /// Indicates whether the current object is equal to another object.
        /// The object is equal iff the internal buffer contents and length are the same.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; 
        /// otherwise, false.
        /// </returns>
        public bool Equals(IArrayBlock other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (!ReferenceEquals(other, this))
            {
                if (this.Length != other.Length)
                {
                    return false;
                }

                for (var i = 0; i < this.Length; i++)
                {
                    if (this[i] != other[i])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; 
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">The <paramref name="obj"/> 
        /// parameter is null.</exception>
        public override bool Equals(object obj)
        {
            var block = obj as IArrayBlock;
            return block != null && this.Equals(block);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in 
        /// hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return this.id;
        }

        #endregion

        #region EnsureIndex Methods

        /// <summary>
        /// Strong index validation.
        /// </summary>
        /// <param name="idx">The index value.</param>
        private void EnsureIndex(int idx)
        {
            if (idx < 0 || idx >= this.Length)
            {
                throw new ArgumentOutOfRangeException("idx", "Strong version: Index is out of range");
            }
        }

        #endregion        

        #region Release Method

        /// <summary>
        /// Object is returned to pool.
        /// </summary>
        public void Release()
        {
            if (this.arrayBlockPool != null)
            {
                this.arrayBlockPool.Release(this);
            }
        }

        #endregion
    }
}
