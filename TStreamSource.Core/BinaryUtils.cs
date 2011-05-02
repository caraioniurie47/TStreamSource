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

namespace TStreamSource.Core
{
    #region Usings

    using System;
    using System.Collections.Generic;

    using NLog;

    #endregion

    /// <summary>
    /// The binary utils.
    /// </summary>
    public static class BinaryUtils
    {
        #region Logger

        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion

        #region ReadBool

        /// <summary>
        /// Read a bit and convert it to boolean value.
        /// </summary>
        /// <param name="b">The byte to read.</param>
        /// <param name="pos">Position of the bit in byte.</param>
        /// <returns>The bollean value of the extracted bit.</returns>
        public static bool ReadBool(byte b, int pos)
        {
            var res = GetBits(b, pos, 1);
            switch (res)
            {
                case 0:
                    return false;
                case 1:
                    return true;
            }

            throw new CustomException("Unsupported value is returned from GetBits()");
        }

        #endregion

        #region GetBits

        /// <summary>
        /// Return a part from byte.
        /// </summary>
        /// <param name="b">The byte to read.</param>
        /// <param name="offset">The offset, it starts from left.</param>
        /// <param name="count">The number of bits.</param>
        /// <returns>Partial value.</returns>
        public static byte GetBits(byte b, int offset, int count)
        {
            #region GUARD

            Ensure.IsInRange(Log, 0, 7, offset, "offset is not in range [0,7]");
            Ensure.IsInRange(Log, 1, 8, count, "count is not in range [1, 8]");

            #endregion

            return Convert.ToByte((b >> (8 - offset - count)) & ((1 << count) - 1));
        }

        #endregion

        #region GetBitsEx

        /// <summary>
        /// Return a value which can be read across few bytes.
        /// </summary>
        /// <param name="segment">The segment of bytes.</param>
        /// <param name="index">The starting index.</param>
        /// <param name="offset">The bit offset.</param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static long GetBitsEx(ArraySegment<byte> segment, int index, int offset, int count)
        {
            return GetBitsEx(segment.Array, segment.Offset + index, offset, count);
        }

        /// <summary>
        /// Return a value which can be read across few bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes.</param>
        /// <param name="index">The starting index.</param>
        /// <param name="offset">The bit offset.</param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static long GetBitsEx(byte[] bytes, int index, int offset, int count)
        {
            #region GUARD

            Ensure.IsInRange(Log, 0, 7, offset, "offset is not in range [0,7]");
            Ensure.IsInRange(Log, 1, 62, count, "count is not in range [1, 62]");
            Ensure.IsNotNull(Log, bytes, "bytes is null");
            Ensure.IsInRange(Log, 0, bytes.Length - 1, index, "index is out of range");

            #endregion

            #region INIT

            var tupleList = new List<Tuple<byte, int>>();
            var nowLen = count;
            var tmpOffset = offset;
            var tmpIdx = index;

            #endregion

            #region CYCLE

            while (nowLen > 0)
            {
                var tmpLen = nowLen;
                if (tmpLen > 8 - tmpOffset)
                {
                    tmpLen = 8 - tmpOffset;
                }

                nowLen -= tmpLen;

                var b = GetBits(bytes[tmpIdx], tmpOffset, tmpLen);
                tupleList.Add(new Tuple<byte, int>(b, tmpLen));

                tmpOffset = 0;
                tmpIdx++;
            }

            #endregion

            #region GET

            nowLen = count;
            long total = 0;
            foreach (var tuple in tupleList)
            {
                nowLen -= tuple.Second;
                long num = tuple.First << nowLen;
                total |= num;
            }

            #endregion

            return total;
        }

        #endregion

        #region GetSegmentByte

        /// <summary>
        /// Get the byte from segment.
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>The byte.</returns>
        public static byte GetSegmentByte(ArraySegment<byte> segment, int offset)
        {
            return segment.Array[segment.Offset + offset];
        }

        #endregion
    }
}
