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

namespace TStreamSource.Core.Parsers
{
    /// <summary>
    /// It is a generic packet interface.
    /// </summary>
    public interface IPacketInfo
    {
        /// <summary>
        /// Parse all properties.
        /// </summary>
        void ParseAll();

        /// <summary>
        /// Check if packet is valid. Returns list of errors in case
        /// if the packet is invalid.
        /// </summary>
        /// <param name="errors">The list of errors. Null if no errors found.</param>
        /// <returns>true if the packet is valid, otherwise false.</returns>
        bool IsValid(out string[] errors);
    }
}
