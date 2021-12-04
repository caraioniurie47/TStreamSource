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
    using System.Collections.Generic;

    using NLog;
    using TStreamSource.Core.Sources;

    #endregion

    /// <summary>
    /// Analyze the stream and returns its properties.
    /// </summary>
    public sealed class TStreamInfo : IStreamInfo
    {
        #region Logger

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion

        #region Fields

        /// <summary>
        /// The stream source instance.
        /// </summary>
        private IStreamSource streamSource;

        /// <summary>
        /// The parsed substreams.
        /// </summary>
        private readonly List<Tuple<int, SubStreamType>> subStreams = 
            new List<Tuple<int, SubStreamType>>();

        #endregion

        #region Constructors

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="streamSource">The stream source instance.</param>
        public TStreamInfo(IStreamSource streamSource)
        {
            Ensure.IsNotNull(Log, streamSource, "stream source is null");
            this.streamSource = streamSource;
        }

        #endregion

        #region Public Methods

        #region Configure Methods

        #region Configure

        /// <summary>
        /// Configure the object.
        /// </summary>
        public void Configure()
        {
            this.CheckDisposed();
            this.CheckConfigured();

            this.VerifyObject();

            //this.streamSource.Require(4000);
            this.ParseStream();

            this.IsConfigured = true;
        }

        #endregion

        #region IsConfigured

        private bool isConfigured;

        /// <summary>
        /// Returns true if the current object configured, otherwise false.
        /// </summary>
        public bool IsConfigured
        {
            get { return isConfigured; }
            private set
            {
                CheckConfigured();
                isConfigured = value;
            }
        }

        #endregion

        #region CheckConfigured

        /// <summary>
        /// Throws an exception if the object is configured.
        /// </summary>
        public void CheckConfigured()
        {
            if (IsConfigured)
            {
                throw new CustomException("The object is already configured.");
            }
        }

        #endregion

        #region CheckNonConfigured

        /// <summary>
        /// Throws an exception if the object is not configured.
        /// </summary>
        public void CheckNonConfigured()
        {
            if (!IsConfigured)
            {
                throw new CustomException("The object is not configured.");
            }
        }

        #endregion

        #endregion

        #region GetTopVideoInfo

        /// <summary>
        /// Gets the first video sub-stream.
        /// </summary>
        /// <returns>Returns PID and video type.</returns>
        public Tuple<int, SubStreamType> GetTopVideoInfo()
        {
            this.CheckDisposed();
            this.CheckNonConfigured();

            foreach (var subStream in subStreams)
            {
                if (subStream.Second == SubStreamType.VideoMP1 ||
                    subStream.Second == SubStreamType.VideoMP2 ||
                    subStream.Second == SubStreamType.VideoMP4 ||
                    subStream.Second == SubStreamType.VideoH264)
                {
                    return new Tuple<int, SubStreamType>(subStream.First, subStream.Second);
                }
            }
            
            throw new CustomException("Video substream is not found.");
        }

        #endregion

        #region GetTopAudioInfo

        /// <summary>
        /// Gets the first audio sub-stream.
        /// </summary>
        /// <returns>Returns PID and audio type.</returns>
        public Tuple<int, SubStreamType> GetTopAudioInfo()
        {
            this.CheckDisposed();
            this.CheckNonConfigured();

            foreach (var subStream in subStreams)
            {
                if (subStream.Second == SubStreamType.AudioMP1 ||
                    subStream.Second == SubStreamType.AudioMP2 ||
                    subStream.Second == SubStreamType.AudioDTS ||
                    subStream.Second == SubStreamType.AudioAAC ||
                    subStream.Second == SubStreamType.AudioAC3)
                {
                    return new Tuple<int, SubStreamType>(subStream.First, subStream.Second);
                }
            }

            throw new CustomException("Audio substream is not found.");
        }

        #endregion

        #endregion

        #region Helper Methods

        #region VerifyObject

        /// <summary>
        /// Check if the object is valid.
        /// </summary>
        private void VerifyObject()
        {
            Ensure.IsNotNull(Log, this.streamSource, "stream source is null");
        }

        #endregion

        #region ParseStream

        /// <summary>
        /// Parse stream and search for A / V types.
        /// </summary>
        private void ParseStream()
        {
            subStreams.Clear();

            #region Looking for PAT

            var count = 0;
            var packet = GetPacket();
            while (packet == null || packet.PID != 0)
            {
                packet = GetPacket();
                count++;

                if (count > 4000)
                {
                    throw new CustomException("PAT is not found.");
                }
            }

            #endregion

            #region Parse PAT (Program Association Table)

            var table = GetEntireTable(packet, 0x00);
            if (table == null)
            {
                throw new CustomException("PAT is not parsed");
            }

            var pTable = new TSPacketInfo(table);

            var sectionLen = BinaryUtils.GetBitsEx(pTable.Segment, pTable.DataBytePos + 2, 4, 12);
            //var transportStreamId = BinaryUtils.GetBitsEx(pTable.Bytes, pTable.DataBytePos + 4, 0, 16);
            //var patVersion = BinaryUtils.GetBitsEx(pTable.Bytes, pTable.DataBytePos + 6, 2, 5);

            var infoLen = sectionLen - 5 - 4;
            var numOfId = infoLen / 4;

            var programs = new List<Tuple<long, long>>();
            for (var i = 0; i < numOfId; i++)
            {
                var progNum = BinaryUtils.GetBitsEx(pTable.Segment, pTable.DataBytePos + 9 + 4 * i, 0, 16);
                var pid = BinaryUtils.GetBitsEx(pTable.Segment, pTable.DataBytePos + 11 + 4 * i, 3, 13);

                if (progNum == 0)
                {
                    //var networkId = pid;
                }
                else
                {
                    programs.Add(new Tuple<long, long>(progNum, pid));
                }
            }

            #endregion

            #region Parse PMT (Program Map Table)

            Ensure.IsNotZero(Log, programs.Count, "No programs are found");
            count = 0;
            //var list = new List<int>();
            byte[] pmtTable;

            while (true)
            {
                #region GUARD (COUNT)

                count++;

                if (count > 20000)
                {
                    //break;
                    throw new CustomException("PMT is not found.");
                }

                #endregion

                #region SEARCH PMT

                var nextPacket = this.GetPacket();
                if (nextPacket == null)
                {
                    continue;
                }

                pmtTable = this.GetEntireTable(nextPacket, 0x02);
                if (pmtTable == null)
                {
                    continue;
                }

                //list.Add(nextPacket.PID);
                //continue;

                #endregion

                #region CHECK PROGRAMS

                var found = false;
                foreach (var prog in programs)
                {
                    if (prog.Second == nextPacket.PID)
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    break;
                }

                #endregion
            }

            #endregion

            var pmtPacket = new TSPacketInfo(pmtTable);
            sectionLen = BinaryUtils.GetBitsEx(pmtPacket.Segment, pmtPacket.DataBytePos + 2, 4, 12);
            var programInfoLen = BinaryUtils.GetBitsEx(pmtPacket.Segment, pmtPacket.DataBytePos + 11, 4, 12);

            var pmtInfoLen = sectionLen - 13 - programInfoLen;
            var pmtStartPos = Convert.ToInt32(pmtPacket.DataBytePos + 13 + programInfoLen);

            while (pmtInfoLen > 0)
            {
                var streamType = BinaryUtils.GetBitsEx(pmtPacket.Segment, pmtStartPos, 0, 8);
                var ePID = Convert.ToInt32(BinaryUtils.GetBitsEx(pmtPacket.Segment, pmtStartPos + 1, 3, 13));
                var eLen = Convert.ToInt32(BinaryUtils.GetBitsEx(pmtPacket.Segment, pmtStartPos + 3, 4, 12));
                
                SubStreamType subStreamType = SubStreamType.None;
                switch (streamType)
                {
                    case 0x02:
                        subStreamType = SubStreamType.VideoMP2;
                        break;
                    case 0x03:
                    case 0x04:
                        subStreamType = SubStreamType.AudioMP2;
                        break;
                    case 0x06:

                        // DTS checking
                        if (CheckDescriptorES(pmtPacket.Segment, pmtStartPos + 5, eLen, 0x73))
                        {
                            subStreamType = SubStreamType.AudioDTS;
                        }
                        // Subtitle checking
                        else if (CheckDescriptorES(pmtPacket.Segment, pmtStartPos + 5, eLen, 0x59))
                        {
                            subStreamType = SubStreamType.Subtitle;
                        }
                        // AC3 checking
                        else if (CheckDescriptorES(pmtPacket.Segment, pmtStartPos + 5, eLen, 0x6a))
                        {
                            subStreamType = SubStreamType.AudioAC3;
                        }
                        // Teletext checking
                        else if (CheckDescriptorES(pmtPacket.Segment, pmtStartPos + 5, eLen, 0x56))
                        {
                            subStreamType = SubStreamType.Teletext;
                        }

                        break;
                    case 0x0B:
                        subStreamType = SubStreamType.Subtitle;
                        break;
                    case 0x0F:
                    case 0x11:
                        subStreamType = SubStreamType.AudioAAC;
                        break;
                    case 0x10:
                        subStreamType = SubStreamType.VideoMP4;
                        break;
                    case 0x1b:
                        subStreamType = SubStreamType.VideoH264;
                        break;
                    
                    case 0x81:
                    case 0x83:
                    case 0x85:
                    case 0x8A:
                        subStreamType = SubStreamType.AudioAC3;
                        break;
                }

                subStreams.Add(new Tuple<int, SubStreamType>(ePID, subStreamType));

                pmtInfoLen -= (eLen + 5);
                pmtStartPos += (eLen + 5);
            }
        }

        #endregion

        #region CheckDescriptorES

        /// <summary>
        /// Check ES descriptor for provided tag.
        /// </summary>
        /// <param name="segment">The segment of bytes to check.</param>
        /// <param name="pos">The position within array.</param>
        /// <param name="len">The length of the descriptor section.</param>
        /// <param name="tag">The descriptor tag.</param>
        /// <returns>True if the descriptor is found, otherwise false.</returns>
        private static bool CheckDescriptorES(ArraySegment<byte> segment, int pos, int len, byte tag)
        {
            var remainingLen = len;
            var newPos = pos;

            while (remainingLen > 0)
            {
                byte dTag = BinaryUtils.GetSegmentByte(segment, newPos);
                byte dLen = BinaryUtils.GetSegmentByte(segment, newPos + 1);

                if (dTag == tag)
                {
                    return true;
                }

                newPos += (dLen + 2);
                remainingLen -= (dLen + 2);

            }
            
            return false;
        }

        #endregion

        #region GetEntireTable

        /// <summary>
        /// Gets the byte section of the table.
        /// </summary>
        /// <param name="packet">The starting packet for the table.</param>
        /// <param name="tableId">TableId</param>
        /// <returns>Section of bytes.</returns>
        private byte[] GetEntireTable(TSPacketInfo packet, int tableId)
        {
            Ensure.IsNotNull(Log, packet, "packet is null");

            if (!packet.PayloadUnitStartIndicator || !packet.HasPayload)
            {
                //throw new CustomException("The packet doesn't contain a table section.");
                return null;
            }

            var pointerField = packet.GetDataByte(0);
            if (pointerField != 0)
            {
                //Ensure.IsZero(Log, pointerField, "pointer field must be zero.");
                return null;
            }
            
            // TODO: move to PATSection
            var pTableId = packet.GetDataByte(1);
            if (tableId != pTableId)
            {
                //Ensure.AreEqual(Log, tableId, pTableId, "TableId is invalid.");
                return null;
            }
            
            var sectionSyntaxIndicator = BinaryUtils.ReadBool(packet.GetDataByte(2), 0);
            if (!sectionSyntaxIndicator)
            {
                //Ensure.IsTrue(Log, sectionSyntaxIndicator, "sectionSyntaxIndicator is not true.");
                return null;
            }
            
            var padding = BinaryUtils.ReadBool(packet.GetDataByte(2), 1);
            if (padding)
            {
                //Ensure.IsFalse(Log, padding, "padding is not false.");    
                return null;
            }
            
            var reserved = BinaryUtils.GetBits(packet.GetDataByte(2), 2, 2);
            if (reserved != 3)
            {
                //Ensure.AreEqual(Log, 3, reserved, "reserved is not 11b (3).");    
                return null;
            }

            var sectionLen = BinaryUtils.GetBitsEx(packet.Segment, packet.DataBytePos + 2, 4, 12);
            sectionLen = Math.Min(4096, sectionLen) - (Constants.TSPacketSize - packet.DataBytePos - 3); // TODO: Review.

            var sectionData = new byte[4096];
            var sectionOffset = 0;

            Buffer.BlockCopy(packet.Segment.Array, packet.Segment.Offset, sectionData, sectionOffset, packet.Segment.Count);
            sectionOffset += packet.Segment.Count;

            while (sectionLen > 0)
            {
                var nextPacket = this.GetPacket();
                if (nextPacket == null)
                {
                    continue;
                }

                if (!nextPacket.PayloadUnitStartIndicator &&
                    nextPacket.AdaptationFieldControl == AdaptationType.Payload &&
                    nextPacket.PID == packet.PID)
                {
                    var len = nextPacket.Segment.Count - 4;
                    Buffer.BlockCopy(nextPacket.Segment.Array, nextPacket.Segment.Offset + 4, sectionData, sectionOffset, len);
                    sectionOffset += len;
                    sectionLen -= len;
                }
            }

            return sectionData;
        }

        #endregion

        #region GetPacket

        /// <summary>
        /// Return the TS packet.
        /// </summary>
        /// <returns>TS packet.</returns>
        private TSPacketInfo GetPacket()
        {
            var segment = this.streamSource.Read(1);
            return segment == null ? null : new TSPacketInfo(segment.Value);
        }

        #endregion

        #endregion

        #region IDisposable Members

        private bool disposed;

        #region Finalizer

        /// <summary>
        /// The Finalizer.
        /// </summary>
        ~TStreamInfo()
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
                this.streamSource = null;
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
