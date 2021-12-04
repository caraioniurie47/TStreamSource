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

namespace TStreamSource.Core.Parsers
{
    #region Usings

    using System;

    using NLog;

    #endregion

    /// <summary>
    /// Transport Stream adaptation field.
    /// </summary>
    public sealed class TSAdaptation
    {
        #region Logger

        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion

        #region Fields

        /// <summary>
        /// The packet segment.
        /// </summary>
        private readonly ArraySegment<byte> segment;

        #endregion

        #region Constructors

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="segment">The segment.</param>
        public TSAdaptation(ArraySegment<byte> segment)
        {
            #region GUARD

            Ensure.IsNotNull(Log, segment, "segment is null");

            #endregion

            this.segment = segment;
        }

        #endregion

        #region Properties

        #region Length

        private byte length;
        private bool hasLength;

        /// <summary>
        /// The adaptation_field_length is an 8-bit value.
        /// ISO: [adaptation_field_length]
        /// </summary>
        public byte Length
        {
            get
            {
                if (!hasLength)
                {
                    length = BinaryUtils.GetSegmentByte(segment, 4);
                    hasLength = true;
                }

                return length;
            }
        }

        #endregion

        #endregion
    }
}
