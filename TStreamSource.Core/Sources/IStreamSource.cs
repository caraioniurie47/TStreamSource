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
    using TStreamSource.Core.Parsers;

    #endregion

    /// <summary>
    /// The classes which implement this interface acts as the source of data.
    /// </summary>
    public interface IStreamSource : IConfigurator
    {
        #region Properties

        /// <summary>
        /// The length of packet.
        /// </summary>
        int PacketLength { get; set; }

        /// <summary>
        /// Gets or sets the method which is used for packet identification.
        /// </summary>
        SearchPacketDelegate SearchMethod { get; set; }

        #endregion

        #region Read Methods

        /// <summary>
        /// Read from stream one or few packets.
        /// </summary>
        /// <returns>The array segment of found bytes, or null if no packets are found.</returns>
        ArraySegment<byte>? Read(int count);
        
        /// <summary>
        /// Reads a number of packets into buffer.
        /// </summary>
        /// <param name="buffer">Pointer to the buffer.</param>
        /// <param name="count">The number of packets to read.</param>
        /// <returns>The total number of bytes read into the buffer. or zero (0) if the end of the stream has been reached.</returns>
        int Read(IntPtr buffer, int count);

        #endregion

        #region Require

        /// <summary>
        /// Block calling thread until the source has requested number of packets.
        /// </summary>
        /// <param name="count">The required number of packets.</param>
        /// <returns>true if number of requested packets are available, otherwise false.</returns>
        bool Require(int count);

        #endregion

        #region Reset

        /// <summary>
        /// Reset stream to start.
        /// </summary>
        void Reset();

        #endregion
    }
}
