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

#include "Source.h"
#include "Guids.h"

CPushPinSource::CPushPinSource(HRESULT *phr, CSource *pFilter) : 
		CSourceStream(NAME("Push Pin"), phr, pFilter, L"OUT"),
		m_Callback(NULL),
		m_lBufferSize(0)
{
}

CPushPinSource::~CPushPinSource()
{
    if (m_Callback != NULL)
    {
	    m_Callback->Release();
    }
}

HRESULT CPushPinSource::GetMediaType(CMediaType *pMediaType)
{
	HRESULT hr = S_OK;

    CheckPointer(pMediaType, E_POINTER);
    CAutoLock cAutoLock(m_pFilter->pStateLock());

    if (m_amt.IsValid())
    {
		hr = pMediaType->Set(m_amt);
	}
	else
	{
        hr = E_FAIL;
    }

	return hr;
}

HRESULT CPushPinSource::DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest)
{
    HRESULT hr;

    CheckPointer(pAlloc, E_POINTER);
    CheckPointer(pRequest, E_POINTER);

    CAutoLock cAutoLock(m_pFilter->pStateLock());

    if (!m_amt.IsValid())
    {
        return E_FAIL;
    }

    if (pRequest->cBuffers == 0)
    {
        pRequest->cBuffers = 2;
    }

	pRequest->cbBuffer = m_lBufferSize;

    ALLOCATOR_PROPERTIES Actual;
    hr = pAlloc->SetProperties(pRequest, &Actual);
    if (SUCCEEDED(hr))
    {
		// Is this allocator unsuitable?
		if (Actual.cbBuffer < pRequest->cbBuffer)
		{
			hr = E_FAIL;
		}
	}

    return hr;
}

HRESULT CPushPinSource::FillBuffer(IMediaSample *pSample)
{
	HRESULT hr = S_OK;

    CheckPointer(pSample, E_POINTER);
	CheckPointer(m_Callback, E_POINTER);

    CAutoLock cAutoLock(m_pFilter->pStateLock());

	hr = m_Callback->MediaSampleCB(pSample);

	return hr;
}

STDMETHODIMP CPushPinSource::NonDelegatingQueryInterface(REFIID riid, void **ppv)
{
    if (riid == IID_TStreamSourceFilterConfig)
    {
        return GetInterface((ITStreamSourceFilterConfig*) this, ppv);
    }

    return CSourceStream::NonDelegatingQueryInterface(riid, ppv);
}

STDMETHODIMP CPushPinSource::SetMediaType(AM_MEDIA_TYPE *amt, long lBufferSize)
{
	HRESULT hr = S_OK;
    CAutoLock cAutoLock(m_pFilter->pStateLock());
	
    if (!m_amt.IsValid())
    {
		if (!IsEqualGUID(amt->majortype, GUID_NULL))
		{
			hr = m_amt.Set(*amt);
			m_lBufferSize = lBufferSize;
		}
		else
		{
			hr = MAKE_HRESULT(1, FACILITY_WIN32, ERROR_INVALID_PARAMETER);
		}
	}
	else
	{
        hr = MAKE_HRESULT(1, FACILITY_WIN32, ERROR_ALREADY_INITIALIZED);
    }

	return hr;
}

STDMETHODIMP CPushPinSource::SetSampleCB(ITStreamSourceSampleCB *pfn)
{
	HRESULT hr = S_OK;

    CheckPointer(pfn, E_POINTER);
    CAutoLock cAutoLock(m_pFilter->pStateLock());

    if (m_amt.IsValid())
    {
		m_Callback = pfn;
		m_Callback->AddRef();
	}
	else
	{
        hr = E_FAIL;
    }

    return hr;
}

/**********************************************
 *
 *  CPushSource Class
 *
 **********************************************/

CPushSource::CPushSource(IUnknown *pUnk, HRESULT *phr)
           : CSource(NAME("Push Source"), pUnk, CLSID_TStreamSourceFilter)
{
    m_pPin = new CPushPinSource(phr, this);

    if (phr)
    {
        if (m_pPin == NULL)
        {
            *phr = E_OUTOFMEMORY;
        }
        else
        {
            *phr = S_OK;
        }
    }

	m_pClock = new CSimpleClock(NAME(""), GetOwner(), phr);
	if (m_pClock == NULL)
	{
		if (phr)
		{
			*phr = E_OUTOFMEMORY;
		}

		return;
	}
}

CPushSource::~CPushSource()
{
	if (m_pClock)
	{
		delete m_pClock;
	}

    if (m_pPin)
	{
		delete m_pPin;
	}
}

CUnknown * WINAPI CPushSource::CreateInstance(IUnknown *pUnk, HRESULT *phr)
{
    CPushSource *pNewFilter = new CPushSource(pUnk, phr );

    if (phr)
    {
        if (pNewFilter == NULL)
        {
            *phr = E_OUTOFMEMORY;
        }
        else
        {
            *phr = S_OK;
        }
    }

    return pNewFilter;
}

STDMETHODIMP CPushSource::NonDelegatingQueryInterface(REFIID riid, void ** ppv)
{
	CheckPointer(ppv ,E_POINTER);

	if (riid == IID_IReferenceClock)
	{
		return GetInterface((IReferenceClock*)m_pClock, ppv);
	}

	return CSource::NonDelegatingQueryInterface(riid, ppv);
}