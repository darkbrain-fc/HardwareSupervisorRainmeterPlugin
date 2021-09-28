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
using System;
using System.Collections.Generic;
using System.Management;
using System.Windows.Forms;

namespace Configurator
{
    public partial class ConfiguratorForm : Form
    {
        private static string NAMESPACE = "root\\HardwareSupervisor";
        private static string QUERY_HW = "SELECT * FROM Hardware";
        private Dictionary<string, string> m_hardware = new Dictionary<string, string>();
        private SortableBindingList<Wrapper> m_data = new SortableBindingList<Wrapper>();
        public ConfiguratorForm()
        {
            InitializeComponent();
            Grid.AutoGenerateColumns = false;            
            Grid.DataSource = m_data;
            ManagementObjectSearcher searchHW =
                new ManagementObjectSearcher(NAMESPACE, QUERY_HW);

            foreach (ManagementObject queryObj in searchHW.Get())
            {
                m_hardware.Add(queryObj["Identifier"].ToString(), queryObj["Name"].ToString());
            }
        }

        private void SearchButtonClick(object sender, EventArgs e)
        {
            try
            {
                ManagementObjectSearcher searchSensors =
                    new ManagementObjectSearcher(NAMESPACE, QueryText.Text);

                Grid.Visible = false;
                m_data.Clear();
                foreach (ManagementObject queryObj in searchSensors.Get())
                {
                    string parent = queryObj["Parent"].ToString();
                    m_hardware.TryGetValue(parent, out string name);
                    m_data.Add(new Wrapper(name, queryObj));
                }
                this.Grid.Sort(Grid.Columns[0], System.ComponentModel.ListSortDirection.Ascending);
            }
            catch (ManagementException error)
            {
                MessageBox.Show("An error occurred while querying for WMI data: " + error.Message);
            }
            finally 
            {
                Grid.Visible = true;
            }
        }

        private void GenerateClick(object sender, EventArgs e)
        {
            var ini = new IniFile("hardwaresupervisor.ini");

            // Rainmeter
            ini.Write("Background", "#@#Background.png", "Rainmeter");
            ini.Write("BackgroundMode", "3", "Rainmeter");
            ini.Write("BackgroundMargins", "0,34,0,14", "Rainmeter");

            // meterTitle
            ini.Write("Meter", "String", "meterTitle");
            ini.Write("MeterStyle", "styleTitle", "meterTitle");
            ini.Write("X", "120", "meterTitle");
            ini.Write("Y", "12", "meterTitle");
            ini.Write("W", "190", "meterTitle");
            ini.Write("H", "18", "meterTitle");
            ini.Write("Text", "Hardware Supervisor", "meterTitle");

            // styleTitle
            ini.Write("StringAlign", "Center", "styleTitle");
            ini.Write("StringStyle", "Bold", "styleTitle");
            ini.Write("StringEffect", "Shadow", "styleTitle");
            ini.Write("FontEffectColor", "0,0,0,50", "styleTitle");
            ini.Write("FontColor", "#colorText#", "styleTitle");
            ini.Write("FontFace", "#fontName#", "styleTitle");
            ini.Write("FontSize", "10", "styleTitle");
            ini.Write("AntiAlias", "1", "styleTitle");
            ini.Write("ClipString", "1", "styleTitle");

            // styleLeftText
            ini.Write("StringAlign", "Left", "styleLeftText");
            ini.Write("StringCase", "None", "styleLeftText");
            ini.Write("StringStyle", "Bold", "styleLeftText");
            ini.Write("StringEffect", "Shadow", "styleLeftText");
            ini.Write("FontEffectColor", "0,0,0,20", "styleLeftText");
            //ini.Write("; FontColor", "#colorText#
            //ini.Write("; FontFace =#fontName#
            //ini.Write("; FontSize =#textSize#
            ini.Write("FontColor", "FFFFFF", "styleLeftText");
            ini.Write("AntiAlias", "1", "styleLeftText");
            ini.Write("ClipString", "1", "styleLeftText");

            //[styleRightText]
            //ini.Write("StringAlign = Right
            //ini.Write("StringCase = None
            //ini.Write("StringStyle = Bold
            //ini.Write("StringEffect = Shadow
            //ini.Write("FontEffectColor = 0,0,0,20
            //ini.Write(";FontColor =#colorText#
            //ini.Write(";FontFace =#fontName#
            //ini.Write("; FontSize =#textSize#
            //ini.Write("AntiAlias = 1
            //ini.Write("ClipString = 1

            //;[styleBar]
            //            ; BarColor =#colorBar#
            //; BarOrientation = HORIZONTAL
            // ; SolidColor = 255,255,255,15

            //    ;[styleSeperator]
            //            ; SolidColor = 255,255,255,15

            // Metadata
            ini.Write("Name", "HardwareSupervisor", "Metadata");
            ini.Write("Author", "DarkBrain", "Metadata");
            ini.Write("Information", "Displays Hardware Information", "Metadata");
            ini.Write("License", "Creative Commons BY-NC - SA 3.0", "Metadata");
            ini.Write("Version", "1.0.0", "Metadata");

            // MeasureServiceStatus
            ini.Write("Measure", "Plugin", "MeasureServiceStatus");
            ini.Write("Plugin", "HardwareSupervisor", "MeasureServiceStatus");
            ini.Write("Refresh", "5", "MeasureServiceStatus");
            ini.Write("Namespace", NamespaceText.Text, "MeasureServiceStatus");
            ini.Write("Query", QueryText.Text, "MeasureServiceStatus");

            int i = 0;
            int refresh_time = 5;
            int x = 10;
            int y = 20;
            int width = 260;
            int height = 18;
            string section;

            // Measure
            foreach (Wrapper data in m_data)
            {
                section = "Measure" + i;
                ini.Write("Measure", "Plugin", section);
                ini.Write("Plugin", "HardwareSupervisor", section);
                ini.Write("Refresh", refresh_time.ToString(), section);
                ini.Write("Identifier", data.Identifier, section);
                i++;
            }

            i = 0;
            // Text
            foreach (Wrapper data in m_data)
            {
                y += 20;
                string measure_name = "Measure" + i;
                section = "Text" + measure_name;
                ini.Write("Meter", "STRING", section);
                ini.Write("MeasureName", measure_name, section);
                ini.Write("X", x.ToString(), section);
                ini.Write("Y", y.ToString(), section);
                ini.Write("W", width.ToString(), section);
                ini.Write("H", height.ToString(), section);
                ini.Write("MeterStyle", "styleLeftText", section);
                if (data.Identifier.Contains("temperature", StringComparison.OrdinalIgnoreCase))
                    ini.Write("Text", data.Name + " %1 °C", section);
                else if (data.Identifier.Contains("fan", StringComparison.OrdinalIgnoreCase))
                    ini.Write("Text", data.Name + " %1 rpm", section);
                else if (data.Identifier.Contains("voltage", StringComparison.OrdinalIgnoreCase))
                    ini.Write("Text", data.Name + " %1 v", section);
                else if (data.Identifier.Contains("frequency", StringComparison.OrdinalIgnoreCase))
                    ini.Write("Text", data.Name + " %1 Mhz", section);
                else
                    ini.Write("Text", data.Name + " %1", section);
                i++;
            }            
        }
    }

    class Wrapper { 
        private ManagementObject m_managementObject;
        private string m_name;

        public Wrapper(string name, ManagementObject managementObject)
        {
            m_managementObject = managementObject;
            m_name = name;
        }

        public string Name
        {
            set 
            {
                if (value.Length > 0)
                    m_name = value;
            }
            get
            {
                return m_name;
            }
        }

        public string Identifier
        {
            get
            {
                return m_managementObject["Identifier"].ToString();
            }
        }

        public string Value
        {
            get
            {                
                return m_managementObject["Value"].ToString();
            }
        }
    }    
}
