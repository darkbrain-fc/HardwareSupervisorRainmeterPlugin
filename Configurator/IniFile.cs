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

using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Configurator
{
    class IniFile
    {
        string mPath;
        string mExecutable = Assembly.GetExecutingAssembly().GetName().Name;

        [DllImport("kernel32", EntryPoint = "WritePrivateProfileStringW", CharSet = CharSet.Unicode)]
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32", EntryPoint = "GetPrivateProfileStringW", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        public IniFile(string IniPath = null, bool overwrite = true)
        {
            FileInfo file = new FileInfo(IniPath ?? mExecutable + ".ini");
            mPath = file.FullName;
            if (overwrite)
                file.Delete();
        }

        public string Read(string Key, string Section = null)
        {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section ?? mExecutable, Key, "", RetVal, 255, mPath);
            return RetVal.ToString();
        }

        public void Write(string Key, string Value, string Section = null)
        {
            WritePrivateProfileString(Section ?? mExecutable, Key, Value, mPath);
        }

        public void DeleteKey(string Key, string Section = null)
        {
            Write(Key, null, Section ?? mExecutable);
        }

        public void DeleteSection(string Section = null)
        {
            Write(null, null, Section ?? mExecutable);
        }

        public bool KeyExists(string Key, string Section = null)
        {
            return Read(Key, Section).Length > 0;
        }
    }
}
