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
#include <windows.h>
#include <sstream>
#include "common.h"

static LOG_LEVEL g_log_level = LOG_LEVEL::NO_LOG;
void set_log_level(LOG_LEVEL level)
{
	g_log_level = level;
}

void log(LOG_LEVEL level, const std::wstring& message)
{
	if (level <= g_log_level)
		RmLog((LOGLEVEL)level, message.c_str());
}

Counter getCounter()
{
	Counter counter;
	LARGE_INTEGER li;
	if (!QueryPerformanceFrequency(&li))
		return counter;

	counter.pc_freq = double(li.QuadPart) / 1000.0;

	QueryPerformanceCounter(&li);
	counter.value = li.QuadPart;
	return counter;
}

double getDelta(const Counter& start, const Counter& stop)
{
	return double(stop.value - start.value) / start.pc_freq;
}

bool isNumericValue(VARIANT vtProp)
{
	bool value = false;
	switch (V_VT(&vtProp))
	{
	case VT_I1:
	case VT_I2:
	case VT_I4:
	case VT_I8:
		value = true;
		break;
	case VT_BSTR:
		value = false;
		break;
	case VT_R4:
		value = true;
		break;
	case VT_R8:
		value = true;
		break;
	case VT_BOOL:
		value = false;
		break;
	}
	return value;
}

double getNumericValue(VARIANT vtProp)
{
	double value = -1;
	switch (V_VT(&vtProp))
	{
	case VT_I1:
	case VT_I2:
	case VT_I4:
	case VT_I8:
		value = (double)vtProp.intVal;
		break;
	case VT_R4:
		value = (double)vtProp.fltVal;
		break;
	case VT_R8:
		value = vtProp.dblVal;
		break;
	}
	return value;
}
std::wstring getText(VARIANT vtProp)
{
	std::wstringstream ss;
	switch (V_VT(&vtProp))
	{
	case VT_BSTR:
		ss << std::wstring(vtProp.bstrVal, SysStringLen(vtProp.bstrVal));
		break;
	case VT_BOOL:
		ss << vtProp.boolVal ? L"Yes" : L"No";
		break;
	}
	return ss.str();
}
