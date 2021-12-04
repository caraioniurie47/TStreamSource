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

#pragma once

#include <streams.h>
#include "Clock.h"

/**********************************************
 *
 *  Interface Definitions
 *
 **********************************************/

DECLARE_INTERFACE_(ITStreamSourceSampleCB, IUnknown) {
	STDMETHOD(MediaSampleCB)(THIS_
		IMediaSample *pSample
		) PURE;
};

DECLARE_INTERFACE_(ITStreamSourceFilterConfig, IUnknown) {

    STDMETHOD(SetMediaType) (THIS_
                AM_MEDIA_TYPE *amt,
				long lBufferSize
             ) PURE;

    STDMETHOD(SetSampleCB) (THIS_
                ITStreamSourceSampleCB *pfn
             ) PURE;
};

/**********************************************
 *
 *  Class declarations
 *
 **********************************************/

class CPushPinSource : public CSourceStream, public ITStreamSourceFilterConfig {
protected:
	CMediaType m_amt;
	long m_lBufferSize;
	ITStreamSourceSampleCB *m_Callback;

public:
    CPushPinSource(HRESULT *phr, CSource *pFilter);
    ~CPushPinSource();

    HRESULT GetMediaType(CMediaType *pMediaType);
    HRESULT DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest);
    HRESULT FillBuffer(IMediaSample *pSample);
    
    STDMETHODIMP Notify(IBaseFilter *pSelf, Quality q)
    {
        return E_FAIL;
    }

    STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);
    DECLARE_IUNKNOWN;

	STDMETHODIMP SetMediaType(AM_MEDIA_TYPE *amt, long lBufferSize);
    STDMETHODIMP SetSampleCB(ITStreamSourceSampleCB *pfn);
};

class CPushSource : public CSource {
private:
    CPushSource(IUnknown *pUnk, HRESULT *phr);
    ~CPushSource();

	STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);

    CPushPinSource *m_pPin;
	CSimpleClock *m_pClock;

public:
    static CUnknown * WINAPI CreateInstance(IUnknown *pUnk, HRESULT *phr);  
};

