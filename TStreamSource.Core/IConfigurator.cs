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

namespace TStreamSource.Core
{
    #region Usings

    using System;

    #endregion

    /// <summary>
    /// The generic configurator.
    /// </summary>
    public interface IConfigurator : IDisposable
    {
        /// <summary>
        /// Returns true if the current object configured, otherwise false.
        /// </summary>
        bool IsConfigured { get; }

        /// <summary>
        /// Configure the object.
        /// </summary>
        void Configure();

        /// <summary>
        /// Throws an exception if the object is configured.
        /// </summary>
        void CheckConfigured();

        /// <summary>
        /// Throws an exception if the object is not configured.
        /// </summary>
        void CheckNonConfigured();
    }
}
