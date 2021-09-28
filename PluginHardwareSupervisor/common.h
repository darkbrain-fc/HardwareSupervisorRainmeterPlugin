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
#include "RainmeterAPI.h"
#include <string>

struct Counter {
	Counter() : pc_freq(0), value(-1) {}
	double pc_freq;
	__int64 value;
};

void log(LOGLEVEL level, const std::wstring& message);
Counter getCounter();
double getDelta(const Counter& start, const Counter& stop);
bool isNumericValue(VARIANT vtProp);
double getNumericValue(VARIANT vtProp);
std::wstring getText(VARIANT vtProp);

