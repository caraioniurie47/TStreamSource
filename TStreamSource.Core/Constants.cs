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

namespace TStreamSource.Core
{
    /// <summary>
    /// The constants.
    /// </summary>
    public static class Constants
    {
        #region TSPacketSize

        /// <summary>
        /// The Transport Stream is designed for use in environments where errors are likely, such as storage 
        /// or transmission in lossy or noisy media. Transport Stream packets are 188 bytes in length.
        /// </summary>
        public const int TSPacketSize = 188;

        #endregion

        #region TimeToWaitForUdpData

        /// <summary>
        /// Time to wait for udp data.
        /// </summary>
        public const int TimeToWaitForUdpData = 1000;

        #endregion

        #region DShowNumberOfPacketsTS

        /// <summary>
        /// The number of packets to send into directshow pipeline.
        /// </summary>
        public const int DShowNumberOfPacketsTS = 30;

        #endregion
    }
}
