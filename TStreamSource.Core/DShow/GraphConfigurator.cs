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

namespace TStreamSource.Core.DShow
{
    #region Usings

    using System;
    using System.Runtime.InteropServices;

    using DirectShowLib;
    using DirectShowLib.BDA;

    using NLog;

    using TStreamSource.Core.Parsers;
    using TStreamSource.Core.Sources;

    #endregion

    /// <summary>
    /// Graph configurator implementation.
    /// </summary>
    public sealed class GraphConfigurator : IGraphConfigurator
    {
        #region Logger

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion

        #region Fields

        /// <summary>
        /// The media sample handler instance.
        /// </summary>
        private ISampleHandler sampleHandler;

        #endregion

        #region Properties

        #region Source

        private IStreamSource source;

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        public IStreamSource Source
        {
            get { return source; }
            set
            {
                this.CheckConfigured();
                source = value;
            }
        }

        #endregion

        #region StreamInfo

        private IStreamInfo streamInfo;

        /// <summary>
        /// Gets or sets the stream info.
        /// </summary>
        public IStreamInfo StreamInfo
        {
            get { return streamInfo; }
            set
            {
                this.CheckConfigured();
                streamInfo = value;
            }
        }

        #endregion

        #region Graph

        private IFilterGraph2 graph;
        
        /// <summary>
        /// The directshow graph instance.
        /// </summary>
        public IFilterGraph2 Graph
        {
            get { return graph; }
            set
            {
                this.CheckConfigured();
                graph = value;
            }
        }

        #endregion

        #region VideoRender

        private IBaseFilter videoRender;

        /// <summary>
        /// Gets or sets the graph video renderer instance.
        /// </summary>
        public IBaseFilter VideoRender
        {
            get { return videoRender; }
            set
            {
                this.CheckConfigured();
                this.videoRender = value;
            }
        }

        #endregion

        #region AudioRender

        private IBaseFilter audioRender;

        /// <summary>
        /// Gets or sets the graph audio renderer instance.
        /// </summary>
        public IBaseFilter AudioRender
        {
            get { return audioRender; }
            set
            {
                this.CheckConfigured();
                this.audioRender = value;
            }
        }

        #endregion

        #region VideoGrabber

        private IBaseFilter videoGrabber;

        /// <summary>
        /// Gets or sets the graph video grabber instance.
        /// </summary>
        public IBaseFilter VideoGrabber
        {
            get { return videoGrabber; }
            set
            {
                this.CheckConfigured();
                this.videoGrabber = value;
            }
        }

        #endregion

        #region AudioGrabber

        private IBaseFilter audioGrabber;

        /// <summary>
        /// Gets or sets the graph audio grabber instance.
        /// </summary>
        public IBaseFilter AudioGrabber
        {
            get { return audioGrabber; }
            set
            {
                this.CheckConfigured();
                this.audioGrabber = value;
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Configure Methods

        /// <summary>
        /// Configure the object.
        /// </summary>
        public void Configure()
        {
            this.CheckDisposed();
            this.CheckConfigured();
            this.Verify();

            // TODO: externalize!
            this.sampleHandler = new TSampleHandler { Source = this.Source };
            this.sampleHandler.Configure();

            this.SetupGraph();

            IsConfigured = true;
        }

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

        #endregion

        #region Helper Methods

        #region Verify

        /// <summary>
        /// Verify if properties are valid. If no then throw an exception.
        /// </summary>
        private void Verify()
        {
            Ensure.IsNotNull(Log, Source, "Source is not set.");
            Ensure.IsNotNull(Log, StreamInfo, "StreamInfo is not set.");

            Ensure.IsNotNull(Log, Graph, "Graph is not set.");
            Ensure.IsNotNull(Log, VideoRender, "VideoRender is not set.");
            Ensure.IsNotNull(Log, AudioRender, "AudioRender is not set.");
            //Ensure.IsNotNull(Log, VideoGrabber, "VideoGrabber is not set.");
            //Ensure.IsNotNull(Log, AudioGrabber, "AudioGrabber is not set.");
        }

        #endregion

        #region SetupGraph

        /// <summary>
        /// Setup the directshow graph.
        /// </summary>
        private void SetupGraph()
        {
            Ensure.IsNotNull(Log, this.sampleHandler, "sampleHandler is null");

            var captureBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();

            try
            {
                // Link capture builder to the filter graph instance.
                var hr = captureBuilder.SetFiltergraph(Graph);
                DsError.ThrowExceptionForHR(hr);

                // Create GSSF
                var gssf = (IBaseFilter)new TStreamSourceGSSF();
                IPin gssfPin = null;
 
                try
                {
                    // Configure output pin of the GSSF
                    gssfPin = DsFindPin.ByDirection(gssf, PinDirection.Output, 0);

                    var config = (ITStreamSourceFilterConfig)gssfPin;
                    sampleHandler.SetMediaType(config);

                    hr = config.SetSampleCB(sampleHandler);
                    DsError.ThrowExceptionForHR(hr);

                    // Add the GSSF filter to the graph.
                    hr = Graph.AddFilter(gssf, "TStreamSource GSSF");
                    DsError.ThrowExceptionForHR(hr);

                    IBaseFilter pMpeg2DemuxFilter = null;
                    IPin demuxInput = null;
                    
                    AMMediaType videoType = null;
                    AMMediaType audioType = null;

                    IPin videoOutPin = null;
                    IPin audioOutPin = null;

                    try
                    {
                        #region ADD DEMUX

                        pMpeg2DemuxFilter = DShowUtils.AddFilterById(Graph,
                            typeof(MPEG2Demultiplexer).GUID, "MPEG2 Demultiplexer");
                        Ensure.IsNotNull(Log, pMpeg2DemuxFilter, "Cannot add MPEG2 Demultiplexer to the graph.");

                        demuxInput = DsFindPin.ByDirection(pMpeg2DemuxFilter, PinDirection.Input, 0);

                        hr = Graph.Connect(gssfPin, demuxInput);
                        DsError.ThrowExceptionForHR(hr);

                        #endregion

                        #region CREATE PINS

                        videoType = GetVideoType();
                        audioType = GetAudioType();

                        var tmpDemux = pMpeg2DemuxFilter as IMpeg2Demultiplexer;
                        Ensure.IsNotNull(Log, tmpDemux, "demuxer is invalid");
                        if (tmpDemux == null) return;

                        hr = tmpDemux.CreateOutputPin(videoType, "Video", out videoOutPin);
                        DsError.ThrowExceptionForHR(hr);
                        Ensure.IsNotNull(Log, videoOutPin, "Video output pin is not created.");

                        hr = tmpDemux.CreateOutputPin(audioType, "Audio", out audioOutPin);
                        DsError.ThrowExceptionForHR(hr);
                        Ensure.IsNotNull(Log, audioOutPin, "Audio output pin is not created.");
                        
                        #endregion

                        #region MAP PINS

                        var mapVideo = videoOutPin as IMPEG2PIDMap;
                        Ensure.IsNotNull(Log, mapVideo, "Video mapping interface is invalid.");
                        if (mapVideo == null) return;

                        var mapAudio = audioOutPin as IMPEG2PIDMap;
                        Ensure.IsNotNull(Log, mapAudio, "Audio mapping interface is invalid.");
                        if (mapAudio == null) return;

                        var pids = new int[1];

                        pids[0] = StreamInfo.GetTopVideoInfo().First;
                        hr = mapVideo.MapPID(1, pids, MediaSampleContent.ElementaryStream);
                        DsError.ThrowExceptionForHR(hr);

                        pids[0] = StreamInfo.GetTopAudioInfo().First;
                        hr = mapAudio.MapPID(1, pids, MediaSampleContent.ElementaryStream);
                        DsError.ThrowExceptionForHR(hr);

                        #endregion

                        #region RENDER PINS

                        hr = captureBuilder.RenderStream(null, MediaType.Video, pMpeg2DemuxFilter, VideoGrabber, videoRender);
                        DsError.ThrowExceptionForHR(hr);

                        hr = captureBuilder.RenderStream(null, MediaType.Audio, pMpeg2DemuxFilter, AudioGrabber, audioRender);
                        DsError.ThrowExceptionForHR(hr);
                        
                        #endregion

                        //var filter = Graph as IMediaFilter;
                        //var refClock = audioRender as IReferenceClock;
                        //filter.SetSyncSource(refClock);

                        var filter = Graph as IMediaFilter;
                        Ensure.IsNotNull(Log, filter, "Cannot retrieve IMediaFilter interface.");
                        var refClock = gssf as IReferenceClock;
                        Ensure.IsNotNull(Log, refClock, "Cannot retrieve IReferenceClock interface.");

                        if (filter != null)
                        {
                            filter.SetSyncSource(refClock);
                        }
                    }
                    finally
                    {
                        #region CLEANUP

                        if (videoType != null)
                        {
                            DsUtils.FreeAMMediaType(videoType);
                        }

                        if (audioType != null)
                        {
                            DsUtils.FreeAMMediaType(audioType);
                        }

                        if (videoOutPin != null)
                        {
                            Marshal.ReleaseComObject(videoOutPin);
                        }

                        if (audioOutPin != null)
                        {
                            Marshal.ReleaseComObject(audioOutPin);
                        }

                        if (demuxInput != null)
                        {
                            Marshal.ReleaseComObject(demuxInput);
                        }

                        if (pMpeg2DemuxFilter != null)
                        {
                            Marshal.ReleaseComObject(pMpeg2DemuxFilter);
                        }

                        #endregion
                    }
                }
                finally
                {
                    if (gssfPin != null)
                    {
                        Marshal.ReleaseComObject(gssfPin);
                    }

                    Marshal.ReleaseComObject(gssf);
                }
            }
            finally
            {
                Marshal.ReleaseComObject(captureBuilder);
            }
        }

        #endregion
         
        #region Get Video & Audio Types

        #region GetVideoType Methods

        #region GetVideoType

        /// <summary>
        /// Return the video type.
        /// </summary>
        /// <returns>Video type.</returns>
        private AMMediaType GetVideoType()
        {
            var videoType = StreamInfo.GetTopVideoInfo().Second;
            switch (videoType)
            {
                case SubStreamType.VideoMP1:
                    return GetVideoMP1();
                case SubStreamType.VideoMP2:
                    return GetVideoMP2();
                case SubStreamType.VideoMP4:
                    return GetVideoMP4();
                case SubStreamType.VideoH264:
                    return GetVideoH264();
            }

            throw new CustomException("Unknown video substream found.");
        }

        #endregion

        #region GetVideoMP1

        /// <summary>
        /// Get the MPEG1 video type.
        /// </summary>
        /// <returns>MPEG1 video type</returns>
        private static AMMediaType GetVideoMP1()
        {
            throw new NotImplementedException("MP1 Video is not supported.");
        }

        #endregion

        #region GetVideoMP2

        /// <summary>
        /// Get the MPEG2 video type.
        /// </summary>
        /// <returns>MPEG2 video type</returns>
        private static AMMediaType GetVideoMP2()
        {
            byte[] mpeg2VideoFormat = {
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0xd0, 0x02, 0x00, 0x00,
	            0x40, 0x02, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0xc0, 0xe1, 0xe4, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x80, 0x1a, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x28, 0x00, 0x00, 0x00,
	            0xd0, 0x02, 0x00, 0x00,
	            0x40, 0x02, 0x00, 0x00,
	            0x00, 0x00,
	            0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0xd0, 0x07, 0x00, 0x00,
	            0x42, 0xd8, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x4c, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,

	            0x00, 0x00, 0x01, 0xb3, 0x2d, 0x02, 0x40, 0x33, 
	            0x24, 0x9f, 0x23, 0x81, 0x10, 0x11, 0x11, 0x12, 
	            0x12, 0x12, 0x13, 0x13, 0x13, 0x13, 0x14, 0x14, 
	            0x14, 0x14, 0x14, 0x15, 0x15, 0x15, 0x15, 0x15, 
	            0x15, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 
	            0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 
	            0x18, 0x18, 0x18, 0x19, 0x18, 0x18, 0x18, 0x19, 
	            0x1a, 0x1a, 0x1a, 0x1a, 0x19, 0x1b, 0x1b, 0x1b, 
	            0x1b, 0x1b, 0x1c, 0x1c, 0x1c, 0x1c, 0x1e, 0x1e, 
	            0x1e, 0x1f, 0x1f, 0x21, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            var result = new AMMediaType();
            result.majorType = MediaType.Video;
            result.subType = MediaSubType.Mpeg2Video;
            result.fixedSizeSamples = true;
            result.temporalCompression = false;
            result.sampleSize = 1;
            result.unkPtr = IntPtr.Zero;

            result.formatType = FormatType.Mpeg2Video;
            result.formatSize = mpeg2VideoFormat.GetLength(0);
            result.formatPtr = Marshal.AllocCoTaskMem(result.formatSize);

            Marshal.Copy(mpeg2VideoFormat, 0, result.formatPtr, result.formatSize);

            return result;
        }

        #endregion

        #region GetVideoMP4

        /// <summary>
        /// Get the MPEG4 video type.
        /// </summary>
        /// <returns>MPEG4 video type</returns>
        private AMMediaType GetVideoMP4()
        {
            byte[] mpeg2VideoFormat = {
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0xd0, 0x02, 0x00, 0x00,
	            0x40, 0x02, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0xc0, 0xe1, 0xe4, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x80, 0x1a, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x28, 0x00, 0x00, 0x00,
	            0xd0, 0x02, 0x00, 0x00,
	            0x40, 0x02, 0x00, 0x00,
	            0x00, 0x00,
	            0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0xd0, 0x07, 0x00, 0x00,
	            0x42, 0xd8, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x4c, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x00,

	            0x00, 0x00, 0x01, 0xb3, 0x2d, 0x02, 0x40, 0x33, 
	            0x24, 0x9f, 0x23, 0x81, 0x10, 0x11, 0x11, 0x12, 
	            0x12, 0x12, 0x13, 0x13, 0x13, 0x13, 0x14, 0x14, 
	            0x14, 0x14, 0x14, 0x15, 0x15, 0x15, 0x15, 0x15, 
	            0x15, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 
	            0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 
	            0x18, 0x18, 0x18, 0x19, 0x18, 0x18, 0x18, 0x19, 
	            0x1a, 0x1a, 0x1a, 0x1a, 0x19, 0x1b, 0x1b, 0x1b, 
	            0x1b, 0x1b, 0x1c, 0x1c, 0x1c, 0x1c, 0x1e, 0x1e, 
	            0x1e, 0x1f, 0x1f, 0x21, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            var result = new AMMediaType();
            result.majorType = MediaType.Video;
            result.subType = new Guid(MakeFourCC('A', 'V', 'C', '1'), 0, 0x10, 
                new byte[] { 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71 });
            result.fixedSizeSamples = false;
            result.temporalCompression = true;
            result.sampleSize = 1;
            result.unkPtr = IntPtr.Zero;

            result.formatType = FormatType.Mpeg2Video;
            result.formatSize = mpeg2VideoFormat.GetLength(0);
            result.formatPtr = Marshal.AllocCoTaskMem(result.formatSize);

            Marshal.Copy(mpeg2VideoFormat, 0, result.formatPtr, result.formatSize);

            return result;
        }

        #endregion

        #region GetVideoH264

        /// <summary>
        /// Get the H264 video type.
        /// </summary>
        /// <returns>H264 video type</returns>
        private static AMMediaType GetVideoH264()
        {
            byte[] h264VideoInfo = {
	            	0x00, 0x00, 0x00, 0x00,
	                0x00, 0x00, 0x00, 0x00,
	                0x00, 0x00, 0x00, 0x00,
	                0x00, 0x00, 0x00, 0x00,
	                0x00, 0x00, 0x00, 0x00,
	                0x00, 0x00, 0x00, 0x00,
	                0x00, 0x00, 0x00, 0x00,
	                0x00, 0x00, 0x00, 0x00,
	                0x00, 0x00, 0x00, 0x00,
	                0x00, 0x00, 0x00, 0x00,
	                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	                0x00, 0x00, 0x00, 0x00,
	                0xD0, 0x02, 0x00, 0x00,
	                0x40, 0x02, 0x00, 0x00,
	                0x00, 0x00,
	                0x00, 0x00,
	                0x68, 0x32, 0x36, 0x34,
	                0x00, 0x00, 0x00, 0x00,
	                0x00, 0x00, 0x00, 0x00,
	                0x00, 0x00, 0x00, 0x00,
	                0x00, 0x00, 0x00, 0x00,
	                0x00, 0x00, 0x00, 0x00,
                };
            
            var result = new AMMediaType();
            result.majorType = MediaType.Video;
            result.subType = new Guid("{34363268-0000-0010-8000-00AA00389B71}");
            result.fixedSizeSamples = false;
            result.temporalCompression = true;
            result.sampleSize = 1;
            result.unkPtr = IntPtr.Zero;

            result.formatType = FormatType.VideoInfo;
            result.formatSize = h264VideoInfo.GetLength(0);
            result.formatPtr = Marshal.AllocCoTaskMem(result.formatSize);

            Marshal.Copy(h264VideoInfo, 0, result.formatPtr, result.formatSize);

            return result;
        }

        #endregion

        #endregion

        #region GetAudioType Methods

        #region GetAudioType

        /// <summary>
        /// Gets the right audio type.
        /// </summary>
        /// <returns>The media type.</returns>
        private AMMediaType GetAudioType()
        {
            var audioType = StreamInfo.GetTopAudioInfo().Second;
            switch (audioType)
            {
                case SubStreamType.AudioMP1:
                    return GetAudioMP1();
                case SubStreamType.AudioMP2:
                    return GetAudioMP2();
                case SubStreamType.AudioAAC:
                    return GetAudioAAC();
                case SubStreamType.AudioAC3:
                    return GetAudioAC3();
                case SubStreamType.AudioDTS:
                    return GetAudioDTS();
            }

            throw new CustomException("Unknown audio substream found.");
        }

        #endregion

        #region GetAudioMP1

        /// <summary>
        /// Get Audio for MPEG1.
        /// </summary>
        /// <returns>MPEG1</returns>
        private static AMMediaType GetAudioMP1()
        {
            byte[] mpeg1AudioFormat = {
	            0x50, 0x00,
	            0x02, 0x00,
	            0x80, 0xBB,	0x00, 0x00,
	            0x00, 0x7D,	0x00, 0x00,
	            0x00, 0x03,
	            0x00, 0x00,
	            0x16, 0x00,
	            0x02, 0x00,
	            0x00, 0xE8,
	            0x03, 0x00,
	            0x01, 0x00,	0x01,0x00,
	            0x01, 0x00,	0x1C, 0x00, 0x00, 0x00,	0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            var result = new AMMediaType
                {
                    majorType = MediaType.Audio,
                    subType = MediaSubType.MPEG1Payload,
                    fixedSizeSamples = true,
                    temporalCompression = false,
                    sampleSize = 1,
                    unkPtr = IntPtr.Zero,
                    formatType = FormatType.WaveEx,
                    formatSize = mpeg1AudioFormat.GetLength(0)
                };

            result.formatPtr = Marshal.AllocCoTaskMem(result.formatSize);

            Marshal.Copy(mpeg1AudioFormat, 0, result.formatPtr, result.formatSize);

            return result;
        }

        #endregion

        #region GetAudioMP2

        /// <summary>
        /// Get Audio for MPEG2.
        /// </summary>
        /// <returns>MPEG2</returns>
        private static AMMediaType GetAudioMP2()
        {
            byte[] mpeg2AudioFormat = {
                0x50, 0x00,
                0x02, 0x00,
                0x80, 0xbb, 0x00, 0x00,
                0x00, 0x7d, 0x00, 0x00,
                0x01, 0x00,
                0x00, 0x00,
                0x16, 0x00,
                0x02, 0x00,
                0x00, 0xe8,
                0x03, 0x00,
                0x01, 0x00, 0x01, 0x00,
                0x01, 0x00, 0x16, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            var result = new AMMediaType();
            result.majorType = MediaType.Audio;
            result.subType = MediaSubType.Mpeg2Audio;
            result.fixedSizeSamples = true;
            result.temporalCompression = false;
            result.sampleSize = 1;
            result.unkPtr = IntPtr.Zero;

            result.formatType = FormatType.WaveEx;
            result.formatSize = mpeg2AudioFormat.GetLength(0);
            result.formatPtr = Marshal.AllocCoTaskMem(result.formatSize);

            Marshal.Copy(mpeg2AudioFormat, 0, result.formatPtr, result.formatSize);

            return result;
        }

        #endregion

        #region GetAudioAAC

        /// <summary>
        /// Get Audio for AAC.
        /// </summary>
        /// <returns>AAC</returns>
        private static AMMediaType GetAudioAAC()
        {
            byte[] aacAudioFormat = {
	            0xFF, 0x00,
	            0x02, 0x00,
	            0xC0, 0x5D, 0x00, 0x00,
	            0x08, 0x2C, 0x00, 0x00,
	            0xDE, 0x1B,
	            0x10, 0x00,
	            0x02, 0x00,
	            0x1B, 0x88
            };

            var result = new AMMediaType();
            result.majorType = MediaType.Audio;
            result.subType = new Guid("{000000FF-0000-0010-8000-00AA00389B71}");
            result.fixedSizeSamples = true;
            result.temporalCompression = false;
            result.sampleSize = 1;
            result.unkPtr = IntPtr.Zero;

            result.formatType = FormatType.WaveEx;
            result.formatSize = aacAudioFormat.GetLength(0);
            result.formatPtr = Marshal.AllocCoTaskMem(result.formatSize);

            Marshal.Copy(aacAudioFormat, 0, result.formatPtr, result.formatSize);

            return result;
        }

        #endregion

        #region GetAudioAC3

        /// <summary>
        /// Get Audio for AC3.
        /// </summary>
        /// <returns>AC3</returns>
        private static AMMediaType GetAudioAC3()
        {
            // TODO: Review.
            byte[] mpeg1AudioFormat = {
	            0x50, 0x00,
	            0x02, 0x00,
	            0x80, 0xBB,	0x00, 0x00,
	            0x00, 0x7D,	0x00, 0x00,
	            0x00, 0x03,
	            0x00, 0x00,
	            0x16, 0x00,
	            0x02, 0x00,
	            0x00, 0xE8,
	            0x03, 0x00,
	            0x01, 0x00,	0x01,0x00,
	            0x01, 0x00,	0x1C, 0x00, 0x00, 0x00,	0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            var result = new AMMediaType();
            result.majorType = MediaType.Audio;
            result.subType = MediaSubType.DolbyAC3;
            result.fixedSizeSamples = true;
            result.temporalCompression = false;
            result.sampleSize = 1;
            result.unkPtr = IntPtr.Zero;

            result.formatType = FormatType.WaveEx;
            result.formatSize = mpeg1AudioFormat.GetLength(0);
            result.formatPtr = Marshal.AllocCoTaskMem(result.formatSize);

            Marshal.Copy(mpeg1AudioFormat, 0, result.formatPtr, result.formatSize);

            return result;
        }

        #endregion

        #region GetAudioDTS

        /// <summary>
        /// Get Audio for DTS.
        /// </summary>
        /// <returns>DTS</returns>
        private static AMMediaType GetAudioDTS()
        {
            byte[] dtsAudioFormat = {
	            0x01, 0x02,
	            0x05, 0x00,
	            0x80, 0xBB, 0x00, 0x00,
	            0x34, 0x18, 0x00, 0x00,
	            0xEE, 0x03,
	            0x00, 0x00,
	            0x00, 0x00,
	            0x00, 0x00
            };

            var result = new AMMediaType();
            result.majorType = MediaType.Audio;
            result.subType = new Guid("{e06d8033-db46-11cf-b4d1-00805f6cbbea}");
            result.fixedSizeSamples = true;
            result.temporalCompression = false;
            result.sampleSize = 1;
            result.unkPtr = IntPtr.Zero;

            result.formatType = FormatType.WaveEx;
            result.formatSize = dtsAudioFormat.GetLength(0);
            result.formatPtr = Marshal.AllocCoTaskMem(result.formatSize);

            Marshal.Copy(dtsAudioFormat, 0, result.formatPtr, result.formatSize);

            return result;
        }

        #endregion

        #endregion

        #endregion

        #region MakeFourCC

        /// <summary>
        /// MakeFourCC
        /// http://msdn.microsoft.com/en-us/library/bb153349(v=vs.85).aspx
        /// </summary>
        /// <param name="ch0">CH0</param>
        /// <param name="ch1">CH1</param>
        /// <param name="ch2">CH2</param>
        /// <param name="ch3">CH3</param>
        /// <returns>FourCC</returns>
        private static int MakeFourCC(int ch0, int ch1, int ch2, int ch3)
        {
            return ((byte)(ch0)|((byte)(ch1) << 8)| ((byte)(ch2) << 16) | ((byte)(ch3) << 24));
        }

        #endregion

        #endregion

        #region IDisposable Members

        private bool disposed;

        #region Finalizer

        /// <summary>
        /// The Finalizer.
        /// </summary>
        ~GraphConfigurator()
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
                this.source = null;
                this.streamInfo = null;
                this.graph = null; 
                this.videoRender = null;
                this.audioRender = null;
                this.videoGrabber = null;
                this.audioGrabber = null;
                
                if (this.sampleHandler != null)
                {
                    this.sampleHandler.Dispose();
                    this.sampleHandler = null;
                }

                this.isConfigured = false;
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
