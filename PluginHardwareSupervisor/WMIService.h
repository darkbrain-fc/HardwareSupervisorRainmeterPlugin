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
#pragma once
#include <windows.h>
#pragma comment(lib, "wbemuuid.lib")
#include <comdef.h>
#include <Wbemidl.h>
#include <string>
#include <map>
#include <mutex>

class WMIService
{
private:
	IWbemServices *m_pSvc;
	WMIService(WMIService &other); // no copy
	std::map<std::wstring, std::wstring> m_texts;	
	std::map<std::wstring, double> m_values;
	std::mutex m_data_mutex;

public:
	WMIService();
	~WMIService();

	bool Connect(LPCWSTR wmi_namespace);
	bool IsConnected();
	void Release();
	bool Exec(const std::wstring &wmi_query);
	const std::wstring GetText(const std::wstring& identifier);
	double GetValue(const std::wstring& identifier);
	bool IsValue(const std::wstring& identifier);
	bool IsText(const std::wstring& identifier);
};
