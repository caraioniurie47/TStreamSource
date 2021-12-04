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
    /// <summary>
    /// List of sub-stream types.
    /// </summary>
    public enum SubStreamType
    {
        None = 0,
        VideoMP1 = 1,
        VideoMP2 = 2,
        VideoMP4 = 3,
        VideoH264 = 4,

        AudioMP1 = 5,
        AudioMP2 = 6,
        AudioAAC = 7,
        AudioAC3 = 8,
        AudioDTS = 9,
        
        Subtitle = 10,
        Teletext = 11,
    }
}
