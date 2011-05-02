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
    
    #endregion

    /// <summary>
    /// Array data block.
    /// </summary>
    public interface IArrayBlock : IEquatable<IArrayBlock>, IObjectPoolItem
    {
        #region Internal Buffer

        /// <summary>
        /// The buffer that is used by this block.
        /// </summary>
        /// <value>The buffer.</value>
        byte[] Buffer { get; }
        
        /// <summary>
        /// The offset, in bytes, in the buffer where the block data starts.
        /// </summary>
        int Offset { get; }
        
        /// <summary>
        /// The maximum amount of block data, in bytes, to write or read from the buffer.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Offset-based indexer for byte read/write.
        /// </summary>
        /// <param name="idx">Index, it starts with zero and is relative to offset.</param>
        /// <returns>Byte value from idx position in buffer relative to the offset.</returns>
        byte this[int idx] { get; set; }

        /// <summary>
        /// Pointer to data contained in the block.
        /// </summary>
        IntPtr PtrToData { get; }

        #endregion

        #region Fill Related

        /// <summary>
        /// Fill the entire block's internal buffer with zero value.
        /// </summary>
        void Zero();

        /// <summary>
        /// Fill the entire block's internal buffer with <paramref name="val"/> value.
        /// </summary>
        /// <param name="val">The value.</param>
        void Fill(byte val);

        #endregion
    }
}
