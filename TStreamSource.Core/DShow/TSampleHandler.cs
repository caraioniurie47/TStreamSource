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

    using NLog;

    using TStreamSource.Core.Sources;

    #endregion

    /// <summary>
    /// TS Sample handler implementation.
    /// </summary>
    public sealed class TSampleHandler : ISampleHandler
    {
        #region Logger

        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion

        #region Properties

        #region Source

        private IStreamSource source;
        
        /// <summary>
        /// The stream source.
        /// </summary>
        public IStreamSource Source
        {
            get { return this.source; }
            set
            {
                this.CheckConfigured();
                this.source = value;
            }
        }

        #endregion

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

            Ensure.IsNotNull(Log, Source, "Source is null.");

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

        #region SetMediaType

        /// <summary>
        /// Sets the media type to the filter config object.
        /// </summary>
        /// <param name="config">The filter config object.</param>
        public void SetMediaType(ITStreamSourceFilterConfig config)
        {
            this.CheckDisposed();
            this.CheckNonConfigured();

            var amt = new AMMediaType();
            try
            {
                amt.majorType = MediaType.Stream;
                amt.subType = MediaSubType.Mpeg2Transport;
                amt.formatType = Guid.Empty;

                var hr = config.SetMediaType(amt, Constants.DShowNumberOfPacketsTS * Constants.TSPacketSize);
                DsError.ThrowExceptionForHR(hr);                
            }
            finally
            {
                DsUtils.FreeAMMediaType(amt);    
            }
        }

        #endregion

        #region MediaSampleCB

        /// <summary>
        /// This routine populates the MediaSample.
        /// </summary>
        /// <param name="pSample">Pointer to a sample</param>
        /// <returns>0 = success, 1 = end of stream, negative values for errors</returns>
        public int MediaSampleCB(IMediaSample pSample)
        {
            int hr;

            try
            {
                IntPtr pData;

                hr = pSample.GetPointer(out pData);
                if (hr >= 0)
                {
                    hr = pSample.SetSyncPoint(true);
                    if (hr >= 0)
                    {
                        var len = Source.Read(pData, Constants.DShowNumberOfPacketsTS);
                        if (len != 0)
                        {
                            hr = 0;
                            pSample.SetActualDataLength(len);
                        }
                        else
                        {
                            //hr = 1;
                            hr = 0;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Log.ErrorException("Error in Sample handler.", ex);
                hr = -1;
            }
            finally
            {
                Marshal.ReleaseComObject(pSample);
            }

            return hr;
        }

        #endregion

        #endregion

        #region IDisposable Members

        private bool disposed;

        #region Finalizer

        /// <summary>
        /// The Finalizer.
        /// </summary>
        ~TSampleHandler()
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
