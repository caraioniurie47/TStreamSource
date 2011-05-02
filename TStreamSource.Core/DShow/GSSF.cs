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

    using System.Runtime.InteropServices;
    using DirectShowLib;

    #endregion

    #region TStreamSourceGSSF

    /// <summary>
    /// Interface to GSSF directshow filter.
    /// </summary>
    [ComImport, Guid("98D19ED1-E3BE-4548-8A75-0E502E0F2815")]
	public class TStreamSourceGSSF
	{
    }

    #endregion

    #region ITStreamSourceFilterConfig

    /// <summary>
    /// Interface to GSSF directshow filter config.
    /// </summary>
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("1D4A7364-571A-48f9-AB0F-3823C4798460")]
    public interface ITStreamSourceFilterConfig
    {
        [PreserveSig]
        int SetMediaType([MarshalAs(UnmanagedType.LPStruct)] AMMediaType amt, int lBufferSize);

        [PreserveSig]
        int SetSampleCB(ITStreamSourceSampleCB pfn);
    }

    #endregion

    #region ITStreamSourceSampleCB

    /// <summary>
    /// Interface to GSSF filter callback.
    /// </summary>
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("3CFA1C7C-A074-4ff0-8744-C842248CD265")]
    public interface ITStreamSourceSampleCB
	{
		[PreserveSig]
		int MediaSampleCB(IMediaSample pSample);
    }

    #endregion
}
