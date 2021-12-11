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
#pragma comment(lib, "wbemuuid.lib")
#include <comdef.h>
#include <Wbemidl.h>
#include <string>
#include <sstream>
#include <thread>
#include <mutex>
#include <queue>
#include <condition_variable>
#include <fstream>
#include "RainmeterAPI.h"
#include "WMIService.h"
#include "common.h"

static int g_measuresCnt = 0;
static bool g_run = true;
static std::mutex g_mutex;
static std::condition_variable g_cv;
static std::thread g_worker;
static WMIService g_srv;

struct Measure
{
	Measure() : refreshInterval(60), lastUpdate(0), queued(false) {}

	std::wstring query;
	std::wstring nameSpace;
	std::wstring identifier;
	std::wstring value;
	int refreshInterval;
	int lastUpdate;
	bool queued;
};

static std::queue<Measure*> g_queue;

void QueryWorker()
{
	HRESULT hres;

	hres = CoInitializeEx(0, COINIT_MULTITHREADED);
	if (FAILED(hres))
	{
		log(LVL_ERROR, L"Failed to initialize COM library. Error code = " + std::to_wstring(hres));
		return;
	}

	while (g_run)
	{
		Measure *m;
		{
			std::unique_lock<std::mutex> lock(g_mutex);
			if (g_queue.empty())
				g_cv.wait(lock, [] { return !g_queue.empty() || !g_run; });
			if (!g_run) break;
			m = g_queue.front();
			g_queue.pop();
		} // unlock

		if (!g_srv.IsConnected() && !m->nameSpace.empty())
		{
			g_srv.Connect(m->nameSpace.c_str());
		}

		if (!m->query.empty() && !g_srv.Exec(m->query))
		{
			log(LVL_ERROR, L"Query: " + m->query);
		}
		m->queued = false;
	}
}

void EnqueueForUpdate(Measure *m)
{
	m->queued = true;
	std::unique_lock<std::mutex> lg(g_mutex);
	g_queue.push(m);
	g_cv.notify_one();
}

PLUGIN_EXPORT void Initialize(void** data, void* rm)
{
	if (g_measuresCnt == 0) // first time initialization
	{
		// start WMI query thread
		// waits for main thread to put Measures on g_queue for processing
		g_run = true;
		std::thread worker = std::thread(QueryWorker);
		g_worker.swap(worker);
	}	
	Measure* measure = new Measure;
	*data = measure;
	++g_measuresCnt;
}

PLUGIN_EXPORT void Reload(void* data, void* rm, double* maxValue)
{
	Measure* measure = (Measure*)data;
	measure->refreshInterval = RmReadInt(rm, L"Refresh", 60);
	measure->value = L"";
	measure->query = RmReadString(rm, L"Query", L"");
	int level = RmReadInt(rm, L"LogLevel", 0);
	if (!measure->query.empty()) {
		if (level >= LOG_LEVEL::LVL_ERROR && level <= LOG_LEVEL::LVL_DEBUG)
			set_log_level((LOG_LEVEL)level);
		else
			set_log_level(LOG_LEVEL::NO_LOG);
	}
	measure->nameSpace = RmReadString(rm, L"Namespace", L"");
	measure->identifier = RmReadString(rm, L"Identifier", L"");
	measure->lastUpdate = 0;
}

PLUGIN_EXPORT double Update(void* data)
{
	Measure* measure = (Measure*)data;
	DWORD now = ::GetTickCount() / 1000;
	if (now - (DWORD)measure->lastUpdate >= (DWORD)measure->refreshInterval)
	{
		EnqueueForUpdate(measure);
		measure->lastUpdate = now;
	}
	if (g_srv.IsValue(measure->identifier))
		return g_srv.GetValue(measure->identifier);
	return 0.0;
}

PLUGIN_EXPORT LPCWSTR GetString(void* data)
{
	Measure* measure = (Measure*)data;
	if (measure->identifier.empty())
		return L"#Wait";
	if (g_srv.IsValue(measure->identifier))
		return nullptr;
	if (g_srv.IsText(measure->identifier))
	{
		measure->value = g_srv.GetText(measure->identifier);
		return measure->value.c_str();
	}
	else return L"#Wait";
}

PLUGIN_EXPORT void Finalize(void* data)
{
	Measure* measure = (Measure*)data;
	delete measure;
	--g_measuresCnt;
	// no more wmi measures, release helper
	if (g_measuresCnt == 0)
	{
		// stop queue thread
		g_run = false;
		g_cv.notify_one();
		g_worker.join();
	}
}
