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

#pragma once

#ifndef GUIDS_DEFINED
#define GUIDS_DEFINED

// {98D19ED1-E3BE-4548-8A75-0E502E0F2815}
DEFINE_GUID(CLSID_TStreamSourceFilter, 
0x98d19ed1, 0xe3be, 0x4548, 0x8a, 0x75, 0xe, 0x50, 0x2e, 0xf, 0x28, 0x15);

// {1D4A7364-571A-48f9-AB0F-3823C4798460}
DEFINE_GUID(IID_TStreamSourceFilterConfig, 
0x1d4a7364, 0x571a, 0x48f9, 0xab, 0xf, 0x38, 0x23, 0xc4, 0x79, 0x84, 0x60);

// {3CFA1C7C-A074-4ff0-8744-C842248CD265}
DEFINE_GUID(IID_TStreamSourceSampleCB, 
0x3cfa1c7c, 0xa074, 0x4ff0, 0x87, 0x44, 0xc8, 0x42, 0x24, 0x8c, 0xd2, 0x65);

#endif
