/*
	HardwareSupervisorRainmeterPlugin
	Copyright(C) 2021 Dino Puller

	This program is free software; you can redistribute itand /or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation; either version 2 of the License, or (at
	your option) any later version.

	This program is distributed in the hope that it will be useful, but
	WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the GNU
	General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111 - 1307
	USA.
*/
#include <sstream>
#include "WMIService.h"
#include "ComPtr.h"
#include "RainmeterAPI.h"
#include "common.h"

extern IGlobalInterfaceTable* g_pGIT;

WMIService::WMIService() : m_pSvc(nullptr)
{
}

WMIService::~WMIService()
{
	Release();
}

bool WMIService::IsConnected()
{
	return m_pSvc != nullptr;
}

bool WMIService::Connect(LPCWSTR wmi_namespace)
{
	// release current service, if any
	if (m_pSvc)
	{
		m_pSvc->Release();
		m_pSvc = nullptr;
	}

	HRESULT hres;
	IWbemLocator* locator = nullptr;

	hres = CoCreateInstance(
		CLSID_WbemLocator,
		0,
		CLSCTX_INPROC_SERVER,
		IID_IWbemLocator, (LPVOID*)&locator);

	if (FAILED(hres))
	{
		log(LVL_ERROR, L"Failed to create IWbemLocator object! Error code = " + std::to_wstring(hres));
		return false;
	}

	hres = locator->ConnectServer(
		_bstr_t(wmi_namespace), // Object path of WMI namespace
		NULL,                    // User name. NULL = current user
		NULL,                    // User password. NULL = current
		0,                       // Locale. NULL indicates current
		NULL,                    // Security flags.
		0,                       // Authority (for example, Kerberos)
		0,                       // Context object 
		&m_pSvc                  // pointer to IWbemServices proxy
	);

	if (FAILED(hres))
	{
		log(LVL_ERROR, L"Failed to connect to WMI server! Error code = " + std::to_wstring(hres));
		locator->Release();
		return false;
	}

	// set security levels on the proxy
	hres = CoSetProxyBlanket(
		m_pSvc,                      // Indicates the proxy to set
		RPC_C_AUTHN_WINNT,           // RPC_C_AUTHN_xxx
		RPC_C_AUTHZ_NONE,            // RPC_C_AUTHZ_xxx
		NULL,                        // Server principal name 
		RPC_C_AUTHN_LEVEL_CALL,      // RPC_C_AUTHN_LEVEL_xxx 
		RPC_C_IMP_LEVEL_IMPERSONATE, // RPC_C_IMP_LEVEL_xxx
		NULL,                        // client identity
		EOAC_NONE                    // proxy capabilities 
	);

	if (FAILED(hres))
	{		
		log(LVL_ERROR, L"Failed to set proxy! Error code = " + std::to_wstring(hres));
		m_pSvc->Release();
		m_pSvc = nullptr;
		locator->Release();
	}

	return true;
}

void WMIService::Release()
{
	if (m_pSvc)
	{
		m_pSvc->Release();
		m_pSvc = nullptr;
	}
}

const std::wstring WMIService::GetText(const std::wstring& identifier)
{
	std::unique_lock<std::mutex> lock(m_data_mutex);
	return m_texts[identifier];
}

double WMIService::GetValue(const std::wstring& identifier)
{
	std::unique_lock<std::mutex> lock(m_data_mutex);
	return m_values[identifier];
}

bool WMIService::IsValue(const std::wstring& identifier)
{
	std::unique_lock<std::mutex> lock(m_data_mutex);
	return m_values.find(identifier) != m_values.end();
}

bool WMIService::IsText(const std::wstring& identifier)
{
	std::unique_lock<std::mutex> lock(m_data_mutex);
	return m_texts.find(identifier) != m_texts.end();
}

bool WMIService::Exec(const std::wstring& wmi_query)
{
	if (!m_pSvc)
	{
		log(LVL_ERROR, L"Service error!");
		return false;
	}

	Counter start = getCounter();

	CComPtr<IEnumWbemClassObject> pEnumerator;
	HRESULT hres = m_pSvc->ExecQuery(
		bstr_t("WQL"),
		bstr_t(wmi_query.c_str()),
		WBEM_FLAG_FORWARD_ONLY | WBEM_FLAG_RETURN_IMMEDIATELY | WBEM_FLAG_DIRECT_READ,
		NULL,
		&pEnumerator);

	if (FAILED(hres))
	{
		log(LVL_ERROR, L"WMI query failed: " + wmi_query);
		return false;
	}

	bool has_result = false;
	while (pEnumerator)
	{
		CComPtr<IWbemClassObject> clsObj;
		ULONG ret = 0;
		HRESULT hr = pEnumerator->Next(WBEM_INFINITE, 1, &clsObj, &ret);

		if (ret == 0) break;

		// get first property (non-system, local) of returned object
		// by design there should be only one property
		if (SUCCEEDED(clsObj->BeginEnumeration(WBEM_FLAG_LOCAL_ONLY | WBEM_FLAG_NONSYSTEM_ONLY)))
		{
			std::wstring identifier;
			std::wstring text;
			double value;
			bool is_double = false;
			for (;;) {
				BSTR propName = nullptr;
				VARIANT vtProp = { VT_EMPTY };
				CIMTYPE vtType;
				hr = clsObj->Next(0, &propName, &vtProp, &vtType, NULL);
				if (hr == WBEM_S_NO_MORE_DATA)
					break;

				if (FAILED(hr)) {
					log(LVL_ERROR, L"IWbemClassObject::Next failed! Error code = " + std::to_wstring(hr));
					break;
				}

				if (propName == nullptr)
					continue;

				std::wstringstream ss;
				std::wstring property(propName, SysStringLen(propName));

				if (property.compare(L"Identifier") == 0)
					identifier = getText(vtProp);
				else 
				{
					is_double = isNumericValue(vtProp);
					if (is_double)
						value = getNumericValue(vtProp);
					else
						text = getText(vtProp);
				}
									
				SysFreeString(propName);
				VariantClear(&vtProp);
			}
			clsObj->EndEnumeration();
			{
				std::unique_lock<std::mutex> lock(m_data_mutex);
				if (is_double)
					m_values[identifier] = value;
				else
					m_texts[identifier] = text;
			}
			has_result = true;
		}
	}

	Counter end = getCounter();
	log(LVL_NOTICE, L"Measuring time: " + std::to_wstring(getDelta(start, end)));

	return has_result;
}
