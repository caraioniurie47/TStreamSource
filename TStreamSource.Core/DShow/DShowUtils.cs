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

namespace TStreamSource.Core.DShow
{
    #region Usings

    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    using DirectShowLib;

    using NLog;

    #endregion

    /// <summary>
    /// The DirectShow utils methods. This class is based on DirectShow Utils class.
    /// </summary>
    public static class DShowUtils
    {
        #region Logger

        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion

        #region AddFilterById

        /// <summary>
        /// Add filter identified by CLSID.
        /// </summary>
        /// <param name="graph">Graph instance.</param>
        /// <param name="guid">CLSID of filter.</param>
        /// <param name="name">Name of the filter.</param>
        /// <returns>The filter instance.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static IBaseFilter AddFilterById(IGraphBuilder graph, Guid guid, string name)
        {
            Ensure.IsNotNull(Log, graph, "graph is null");

            IBaseFilter filter = null;

            try
            {
                var type = Type.GetTypeFromCLSID(guid);
                filter = (IBaseFilter)Activator.CreateInstance(type);

                var hr = graph.AddFilter(filter, name);
                DsError.ThrowExceptionForHR(hr);
            }
            catch (Exception ex)
            {
                if (filter != null)
                {
                    graph.RemoveFilter(filter);
                    Marshal.ReleaseComObject(filter);
                    filter = null;
                }

                Log.Fatal(string.Format("Filter {0} is not added to the graph", name) + ex);
            }

            return filter;
        }

        #endregion
    }
}
