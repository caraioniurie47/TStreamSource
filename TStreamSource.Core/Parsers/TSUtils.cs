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

    using System.IO;

    #endregion

    #region SearchPacketDelegate

    /// <summary>
    /// Delegate for packet search.
    /// </summary>
    /// <param name="stream">The stream to look.</param>
    /// <returns>The starting position of packet in the stream, or negative if packet is not found.</returns>
    public delegate int SearchPacketDelegate(Stream stream);

    #endregion

    /// <summary>
    /// TS packet utils methods.
    /// </summary>
    public static class TSUtils
    {
        #region FilterPackets

        /// <summary>
        /// Filter valid packets.
        /// </summary>
        /// <param name="input">The input filename.</param>
        /// <param name="output">The output filename.</param>
        /// <returns>Number of invalid packets.</returns>
        public static int FilterPackets(string input, string output)
        {
            var pack = new byte[188];
            var errors = 0;

            using (var reader = File.Open(input, FileMode.Open, FileAccess.Read))
            using (var writer = File.Open(output, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var len = reader.Read(pack, 0, pack.Length);

                while (len != 0)
                {
                    if (len == pack.Length && (pack[0] == 0x47 && ((pack[1] & 0x80) == 0)))
                    {
                        writer.Write(pack, 0, pack.Length);
                    }
                    else
                    {
                        errors++;
                    }

                    len = reader.Read(pack, 0, pack.Length);
                }
            }

            return errors;
        }

        #endregion

        public static int TSPacketSearch(Stream stream)
        {
            return 0;
        }
    }
}
