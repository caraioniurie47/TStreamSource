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

namespace TStreamSource.Core.Sources
{
    #region Usings

    using System;

    using NLog;

    using TStreamSource.Core.Parsers;

    #endregion

    /// <summary>
    /// Base class for all Sources.
    /// </summary>
    public abstract class AbstractSource : IDisposable
    {
        #region Logger

        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion

        #region Properties

        #region PacketLength

        private int packetLength;

        /// <summary>
        /// The length of packet.
        /// </summary>
        public int PacketLength
        {
            get { return this.packetLength; }    
            set
            {
                this.CheckConfigured();
                this.packetLength = value;
            }
        }

        #endregion

        #region SearchMethod

        private SearchPacketDelegate searchMethod;

        /// <summary>
        /// Gets or sets the method which is used for packet identification.
        /// </summary>
        public SearchPacketDelegate SearchMethod
        {
            get { return this.searchMethod; }
            set
            {
                this.CheckConfigured();
                this.searchMethod = value;
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

            Ensure.IsNotZero(Log, PacketLength, "PacketLength is zero.");
            Ensure.IsNotNull(Log, SearchMethod, "SearchMethod is null.");

            Open();

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

        #region Open and Close Method

        /// <summary>
        /// Cleanup resources.
        /// </summary>
        protected abstract void Open();

        /// <summary>
        /// Cleanup resources.
        /// </summary>
        protected abstract void Close();

        #endregion

        #endregion

        #region IDisposable Members

        private bool disposed;

        #region Finalizer

        /// <summary>
        /// The Finalizer.
        /// </summary>
        ~AbstractSource()
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

        protected virtual void Dispose(bool disposing)
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
        protected void CheckDisposed()
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
