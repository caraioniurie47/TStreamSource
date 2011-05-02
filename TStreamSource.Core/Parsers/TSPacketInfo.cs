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
    #region Usings

    using System;
    using System.Collections.Generic;

    using NLog;

    #endregion

    #region ScramblingType

    /// <summary>
    /// The scrambling types.
    /// </summary>
    public enum ScramblingType
    {
        NotScrambled = 0x00,
        UserDefined1 = 0x01,
        UserDefined2 = 0x02,
        UserDefined3 = 0x03
    }

    #endregion

    #region AdaptationType

    /// <summary>
    /// Adaptation types.
    /// </summary>
    public enum AdaptationType
    {
        Reserved = 0x00,
        Payload = 0x01,
        Adaptation = 0x02,
        AdaptationAndPayload = 0x03
    }

    #endregion

    /// <summary>
    /// Describes a transport packet. Based on ISO/IEC 13818-1:2000(E)
    /// </summary>
    public sealed class TSPacketInfo : IPacketInfo
    {
        #region Logger

        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion

        #region Constructors

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="bytes">Array of bytes.</param>
        public TSPacketInfo(byte[] bytes)
            : this(new ArraySegment<byte>(bytes))
        {
        }

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="segment">The array segment.</param>
        public TSPacketInfo(ArraySegment<byte> segment)
        {
            #region GUARD

            Ensure.IsNotNull(Log, segment, "segment is null");

            #endregion

            this.segment = segment;
        }

        #endregion

        #region Properties

        #region Bytes

        /// <summary>
        /// The packet bytes.
        /// </summary>
        private readonly ArraySegment<byte> segment;

        /// <summary>
        /// Gets the packet bytes.
        /// </summary>
        public ArraySegment<byte> Segment
        {
            get { return this.segment; }
        }

        #endregion

        #region SyncByte

        private byte syncByte;
        private bool hasSyncByte;

        /// <summary>
        /// The sync_byte is a fixed 8-bit field whose value is '01000111' (0x47).
        /// ISO: [sync_byte]
        /// </summary>
        public byte SyncByte
        {
            get
            {
                if (!hasSyncByte)
                {
                    syncByte = BinaryUtils.GetSegmentByte(Segment, 0);
                    hasSyncByte = true;
                }

                return syncByte;
            }
        }

        #endregion

        #region ErrorIndicator

        private bool errorIndicator;
        private bool hasErrorIndicator;

        /// <summary>
        /// When set to true it indicates that at least 1 uncorrectable bit error 
        /// exists in the associated Transport Stream packet. This bit may be set to 
        /// true by entities external to the transport layer. When set to true this 
        /// bit shall not be reset to false unless the bit value(s) in error have 
        /// been corrected.
        /// 
        /// ISO: [transport_error_indicator]
        /// </summary>
        public bool ErrorIndicator
        {
            get
            {
                if (!hasErrorIndicator)
                {
                    errorIndicator = BinaryUtils.ReadBool(BinaryUtils.GetSegmentByte(Segment, 1), 0);
                    hasErrorIndicator = true;
                }

                return errorIndicator;
            }
        }

        #endregion

        #region PayloadUnitStartIndicator

        private bool payloadUnitStartIndicator;
        private bool hasPayloadUnitStartIndicator;

        /// <summary>
        /// This property has normative meaning for Transport Stream packets that 
        /// carry PES packets (refer to 2.4.3.6, ISO) or PSI data (refer to 2.4.4, ISO).
        /// 
        /// When the payload of the Transport Stream packet contains PES packet data, 
        /// the property has the following significance: a <c>true</c> indicates that 
        /// the payload of this Transport Stream packet will commence with the first byte
        /// of a PES packet and a <c>false</c> indicates no PES packet shall start in 
        /// this Transport Stream packet. If the property is set to <c>true</c>, then one 
        /// and only one PES packet starts in this Transport Stream packet. This also applies 
        /// to private streams of stream_type 6 (refer to Table 2-29).
        /// 
        /// When the payload of the Transport Stream packet contains PSI data, the property 
        /// has the following significance: if the Transport Stream packet carries the first 
        /// byte of a PSI section, the property value shall be <c>true</c>, indicating that 
        /// the first byte of the payload of this Transport Stream packet carries the pointer_field. 
        /// If the Transport Stream packet does not carry the first byte of a PSI section, 
        /// the property value shall be <c>false</c>,indicating that there is no pointer_field 
        /// in the payload. Refer to 2.4.4.1 and 2.4.4.2. This also applies to private streams of
        /// stream_type 5 (refer to Table 2-29).
        /// 
        /// For null packets the property value shall be set to <c>false</c>.
        /// The meaning of this bit for Transport Stream packets carrying only private 
        /// data is not defined in this Specification.
        /// 
        /// ISO: [payload_unit_start_indicator]
        /// </summary>
        public bool PayloadUnitStartIndicator
        {
            get
            {
                if (!hasPayloadUnitStartIndicator)
                {
                    payloadUnitStartIndicator = BinaryUtils.ReadBool(BinaryUtils.GetSegmentByte(Segment, 1), 1);
                    hasPayloadUnitStartIndicator = true;
                }

                return payloadUnitStartIndicator;
            }
        }

        #endregion

        #region Priority

        private bool priority;
        private bool hasPriority;

        /// <summary>
        /// When set to <c>true</c> it indicates that the associated packet is of greater priority 
        /// than other packets having the same PID which do not have the property set to <c>true</c>. 
        /// The transport mechanism can use this to prioritize its data within an elementary stream. 
        /// Depending on the application the property may be coded regardless of the PID or within 
        /// one PID only. This field may be changed by channel specific encoders or decoders.
        /// 
        /// ISO: [transport_priority]
        /// </summary>
        public bool Priority
        {
            get
            {
                if (!hasPriority)
                {
                    priority = BinaryUtils.ReadBool(BinaryUtils.GetSegmentByte(Segment, 1), 2);
                    hasPriority = true;
                }

                return priority;
            }
        }

        #endregion

        #region PID

        private short pid;
        private bool hasPID;

        /// <summary>
        /// The PID is a 13-bit field, indicating the type of the data stored in the packet payload. 
        /// PID value 0x0000 is reserved for the Program Association Table (see Table 2-25). 
        /// PID value 0x0001 is reserved for the Conditional Access Table (see Table 2-27). 
        /// PID value 0x0002 is reserved for Transport Stream Description Table
        /// PID values 0x0003 – 0x000F are reserved. 
        /// PID value 0x1FFF is reserved for null packets (see Table 2-3).
        /// 
        /// ISO: [PID]
        /// </summary>
        public short PID
        {
            get
            {
                if (!hasPID)
                {
                    pid = Convert.ToInt16(BinaryUtils.GetBitsEx(this.Segment, 1, 3, 13));
                    hasPID = true;
                }

                return pid;
            }
        }

        #endregion

        #region IsNull

        /// <summary>
        /// Returns <c>true</c> if the packet is null, otherwise <c>false</c>.
        /// </summary>
        public bool IsNull
        {
            get
            {
                return PID == 0x1FFF;
            }
        }

        #endregion

        #region ScramblingControl

        private ScramblingType scramblingControl;
        private bool hasScramblingControl;

        /// <summary>
        /// Indicates the scrambling mode of the Transport Stream packet payload.
        /// The Transport Stream packet header, and the adaptation field when present, shall not be 
        /// scrambled. In the case of a null packet the value of the ScramblingControl
        /// shall be set to NotScrambled.
        /// 
        /// ISO: [transport_scrambling_control]
        /// </summary>
        public ScramblingType ScramblingControl
        {
            get
            {
                if (!hasScramblingControl)
                {
                    scramblingControl = (ScramblingType)BinaryUtils.GetBits(BinaryUtils.GetSegmentByte(Segment, 3), 0, 2);
                    hasScramblingControl = true;
                }

                return scramblingControl;
            }
        }

        #endregion

        #region AdaptationFieldControl

        private AdaptationType adaptationFieldControl;
        private bool hasAdaptationFieldControl;

        /// <summary>
        /// This property indicates whether this Transport Stream packet header is followed by an
        /// adaptation field and/or payload.
        /// 
        /// Decoders shall discard Transport Stream packets with the value set to Reserved. 
        /// In the case of a null packet the value shall be set to Payload.
        /// 
        /// ISO: [adaptation_field_control]
        /// </summary>
        public AdaptationType AdaptationFieldControl
        {
            get
            {
                if (!hasAdaptationFieldControl)
                {
                    adaptationFieldControl = (AdaptationType)BinaryUtils.GetBits(BinaryUtils.GetSegmentByte(Segment, 3), 2, 2);
                    hasAdaptationFieldControl = true;
                }

                return adaptationFieldControl;
            }
        }

        #endregion

        #region HasPayload

        /// <summary>
        /// Returns true if the packet contains payload, otherwise it returns false.
        /// </summary>
        public bool HasPayload
        {
            get
            {
                return AdaptationFieldControl == AdaptationType.Payload ||
                       AdaptationFieldControl == AdaptationType.AdaptationAndPayload;
            }
        }

        #endregion

        #region HasAdaptation

        /// <summary>
        /// Returns true if the packet contains adaptation field, otherwise it returns false.
        /// </summary>
        public bool HasAdaptation
        {
            get
            {
                return AdaptationFieldControl == AdaptationType.Adaptation ||
                       AdaptationFieldControl == AdaptationType.AdaptationAndPayload;
            }
        }

        #endregion

        #region ContinuityCounter

        private byte continuityCounter;
        private bool hasContinuityCounter;

        /// <summary>
        /// The continuity_counter is a 4-bit field incrementing with each Transport Stream packet with the
        /// same PID. The property wraps around to 0 after its maximum value. The property shall not be
        /// incremented when the AdaptationFieldControl of the packet equals '00' or '10'.
        /// 
        /// In Transport Streams, duplicate packets may be sent as two, and only two, consecutive 
        /// Transport Stream packets of the same PID. The duplicate packets shall have the same ContinuityCounter
        /// value as the original packet and the AdaptationFieldControl field shall be equal to '01' or '11'. 
        /// In duplicate packets each byte of the original packet shall be duplicated, with the exception that in 
        /// the program clock reference fields, if present, a valid value shall be encoded.
        /// 
        /// The ContinuityCounter in a particular Transport Stream packet is continuous when it differs by a positive 
        /// value of one from the ContinuityCounter value in the previous Transport Stream packet of the same PID, 
        /// or when either of the nonincrementing conditions (AdaptationFieldControl set to '00' or '10', or duplicate 
        /// packets as described above) are met.
        /// 
        /// The continuity counter may be discontinuous when the discontinuity_indicator is set to '1' (refer to 2.4.3.4). 
        /// In the case of a null packet the value of the ContinuityCounter is undefined.
        /// 
        /// ISO: [continuity_counter]
        /// </summary>
        public byte ContinuityCounter
        {
            get
            {
                if (!hasContinuityCounter)
                {
                    continuityCounter = BinaryUtils.GetBits(BinaryUtils.GetSegmentByte(Segment, 3), 4, 4);
                    hasContinuityCounter = true;
                }

                return continuityCounter;
            }
        }

        #endregion

        #region DataBytePos

        /// <summary>
        /// Gets the starting position for data bytes.
        /// </summary>
        public int DataBytePos
        {
            get
            {
                var pos = 4;
                if (HasAdaptation)
                {
                    pos++;
                    pos += new TSAdaptation(this.Segment).Length;
                }

                return pos;
            }
        }

        #endregion

        #endregion

        #region Methods

        #region ParseAll

        /// <summary>
        /// Parse all properties.
        /// </summary>
        public void ParseAll()
        {
            #pragma warning disable 168
            var p01 = SyncByte;
            var p02 = ErrorIndicator;
            var p03 = PayloadUnitStartIndicator;
            var p04 = Priority;
            var p05 = PID;
            var p06 = ScramblingControl;
            var p07 = AdaptationFieldControl;
            var p08 = ContinuityCounter;
            #pragma warning restore 168
        }

        #endregion

        #region IsValid

        /// <summary>
        /// Check if packet is valid. Returns list of errors in case
        /// if the packet is invalid.
        /// </summary>
        /// <param name="errors">The list of errors. Empty array if no errors found.</param>
        /// <returns>true if the packet is valid, otherwise false.</returns>
        public bool IsValid(out string[] errors)
        {
            var errorsList = new List<string>();

            #region SyncByte Check

            if (SyncByte != 0x47)
            {
                errorsList.Add("SyncByte must be 0x47.");
            }

            #endregion

            #region ScramblingControl Check

            if (IsNull && ScramblingControl != ScramblingType.NotScrambled)
            {
                errorsList.Add("In the case of a null packet the value of the ScramblingControl field shall be set to NotScrambled");
            }

            #endregion

            #region AdaptationFieldControl Check

            if (IsNull && AdaptationFieldControl != AdaptationType.Payload)
            {
                errorsList.Add("In the case of a null packet the value of the AdaptationFieldControl shall be set to Payload.");
            }

            #endregion

            errors = errorsList.ToArray();
            return errors.Length != 0;
        }

        #endregion

        #region GetDataByte

        /// <summary>
        /// Gets the byte from data_byte.
        /// </summary>
        /// <param name="pos">The position in array.</param>
        /// <returns>The byte.</returns>
        public byte GetDataByte(int pos)
        {
            try
            {
                return BinaryUtils.GetSegmentByte(Segment, DataBytePos + pos);
            }
            catch (Exception ex)
            {   
                Log.LogException(LogLevel.Error, "Invalid data", ex);
                throw;
            }
        }

        #endregion

        #endregion
    }
}
