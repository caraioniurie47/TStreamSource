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

namespace TStreamSource.UI
{
    #region Usings

    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    using DirectShowLib;

    using NLog;

    using TStreamSource.Core;
    using TStreamSource.Core.DShow;
    using TStreamSource.Core.Parsers;
    using TStreamSource.Core.Sources;

    #endregion

    public sealed class VideoPlay : IDisposable
    {
        #region Logger

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion

        #region Fields

        private IStreamSource streamSource;
        private IGraphConfigurator configurator;
        private IFilterGraph2 filterGraph;
        private IMediaControl mediaControl;

        private readonly object lockObject = new object();

        #endregion

        #region Constructors

        public VideoPlay(Control hWin)
        {
            try
            {
                SetupGraph(hWin);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        #endregion

        #region Public Methods

        #region Start

        public void Start()
        {
            int hr = this.mediaControl.Run();
            DsError.ThrowExceptionForHR(hr);
        }

        #endregion

        #endregion

        #region Helper Methods

        #region SetupGraph

        private void SetupGraph(Control hWin)
        {
            //streamSource = new UdpSource("127.0.0.1", 2001);
            streamSource = new UdpSource("224.0.0.1", 2002);
            //streamSource = new Core.Sources.FileSource("test.ts");

            streamSource.PacketLength = Constants.TSPacketSize;
            streamSource.SearchMethod = TSUtils.TSPacketSearch;

            streamSource.Configure();

            if (!streamSource.Require(10))
            {
                throw new Exception("Source is not ready. No data is received.");
            }

            Log.Info("Identify the A/V streams");
            var streamInfo = new TStreamInfo(streamSource);
            streamInfo.Configure();
            
            streamSource.Reset();

            filterGraph = new FilterGraph() as IFilterGraph2;
            Ensure.IsNotNull(Log, filterGraph, "graph is null");
            if (filterGraph == null) return;

            var pVideoRender = (IBaseFilter)new VideoRendererDefault();
            var hr = filterGraph.AddFilter(pVideoRender, "Video Renderer");
            DsError.ThrowExceptionForHR(hr);

            var pAudioRender = (IBaseFilter)new DSoundRender();
            hr = filterGraph.AddFilter(pAudioRender, "Audio Render");
            DsError.ThrowExceptionForHR(hr);

            Log.Info("Configure the directshow graph.");
            configurator = new GraphConfigurator
            {
                Source = streamSource,
                StreamInfo = streamInfo,
                Graph = filterGraph,
                VideoRender = pVideoRender,
                AudioRender = pAudioRender,
                VideoGrabber = null,
                AudioGrabber = null
            };

            configurator.Configure();

            var videoWindow = pVideoRender as IVideoWindow;
            ConfigureVideoWindow(videoWindow, hWin);
            
            this.mediaControl = this.filterGraph as IMediaControl;
        }

        #endregion

        #region ConfigureVideoWindow

        private static void ConfigureVideoWindow(IVideoWindow videoWindow, Control hWin)
        {
            int hr = videoWindow.put_Owner(hWin.Handle);
            if (hr >= 0)
            {
                hr = videoWindow.put_WindowStyle((WindowStyle.Child | WindowStyle.ClipChildren | WindowStyle.ClipSiblings));
                DsError.ThrowExceptionForHR(hr);

                hr = videoWindow.put_Visible(OABool.True);
                DsError.ThrowExceptionForHR(hr);

                Rectangle rc = hWin.ClientRectangle;
                hr = videoWindow.SetWindowPosition(0, 0, rc.Right, rc.Bottom);
                DsError.ThrowExceptionForHR(hr);
            }
        }

        #endregion

        #region Close

        private void Close()
        {
            lock (lockObject)
            {
                if (streamSource != null)
                {
                    streamSource.Dispose();
                    streamSource = null;
                }

                if (this.configurator != null)
                {
                    this.configurator.Dispose();
                    this.configurator = null;
                }

                if (this.mediaControl != null)
                {
                    this.mediaControl.Stop();
                    this.mediaControl = null;
                }

                if (this.filterGraph != null)
                {
                    ((IMediaEventSink)this.filterGraph).Notify(EventCode.UserAbort, IntPtr.Zero, IntPtr.Zero);

                    Marshal.ReleaseComObject(this.filterGraph);
                    this.filterGraph = null;
                }
            }

            GC.Collect();
        }

        #endregion

        #endregion

        #region IDisposable Members

        private bool disposed;

        #region Finalizer

        /// <summary>
        /// The Finalizer.
        /// </summary>
        ~VideoPlay()
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
               this.Close();
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
