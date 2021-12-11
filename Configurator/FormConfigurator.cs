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
using System.IO;
using System.Management;
using System.Reflection;
using System.Windows.Forms;

namespace Configurator
{
    public partial class ConfiguratorForm : Form
    {
        private static string NAMESPACE = @"root\HardwareSupervisor";
        private static string QUERY_HW = "SELECT * FROM Hardware";
        private Dictionary<string, string> m_hardware = new Dictionary<string, string>();
        private SortableBindingList<Wrapper> m_data = new SortableBindingList<Wrapper>();
        private System.Windows.Forms.TextBox QueryText;
        private System.Windows.Forms.TextBox NamespaceText;
        private System.Windows.Forms.Button SearchButton;
        private System.Windows.Forms.DataGridView Grid;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnIdentifier;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnValue;
        private System.Windows.Forms.TableLayoutPanel TableLayoutPanel;
        private System.Windows.Forms.Button GenerateButton;
        private System.ComponentModel.IContainer components = null;

        public ConfiguratorForm()
        {
            InitializeComponent();
            Grid.AutoGenerateColumns = false;
            Grid.DataSource = m_data;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void SearchButtonClick(object sender, EventArgs e)
        {
            try {
                ManagementObjectSearcher searchSensors =
                    new ManagementObjectSearcher(NamespaceText.Text, QueryText.Text);

                Grid.Visible = false;
                m_data.Clear();
                foreach (ManagementObject queryObj in searchSensors.Get()) {
                    string parent = queryObj["Parent"].ToString();
                    m_hardware.TryGetValue(parent, out string name);
                    m_data.Add(new Wrapper(name, queryObj));
                }
                this.Grid.Sort(Grid.Columns[0], System.ComponentModel.ListSortDirection.Ascending);
            } catch (ManagementException error) {
                MessageBox.Show("An error occurred while querying for WMI data: " + error.Message);
            } finally {
                Grid.Visible = true;
            }
        }

        private void GenerateClick(object sender, EventArgs e)
        {
            string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string file = docs + @"\Rainmeter\Skins\HardwareSupervisor\hardwaresupervisor.ini";
            if (!File.Exists(file))
                file = "hardwaresupervisor.ini";
            IniFile ini = new IniFile(file);

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
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            ini.Write("Version", version, "Metadata");

            // MeasureServiceStatus
            ini.Write("Measure", "Plugin", "MeasureServiceStatus");
            ini.Write("Plugin", "HardwareSupervisor", "MeasureServiceStatus");
            ini.Write("Refresh", "5", "MeasureServiceStatus");
            ini.Write("Namespace", NamespaceText.Text, "MeasureServiceStatus");
            ini.Write("Query", QueryText.Text, "MeasureServiceStatus");
            ini.Write("LogLevel", "0;  NO_LOG = 0, ERROR = 1, WARNING = 2, NOTICE = 3, DEBUG = 4", "MeasureServiceStatus");
            int i = 0;
            int refresh_time = 5;
            int x = 10;
            int y = 20;
            int width = 260;
            int height = 18;
            string section;

            // Measure
            foreach (Wrapper data in m_data) {
                section = "Measure" + i;
                ini.Write("Measure", "Plugin", section);
                ini.Write("Plugin", "HardwareSupervisor", section);
                ini.Write("Refresh", refresh_time.ToString(), section);
                ini.Write("Identifier", data.Identifier, section);
                i++;
            }

            i = 0;
            // Text
            foreach (Wrapper data in m_data) {
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
                if (data.Identifier.Contains("temperature"))
                    ini.Write("Text", data.Name + " %1 °C", section);
                else if (data.Identifier.ToLower().Contains("fan"))
                    ini.Write("Text", data.Name + " %1 rpm", section);
                else if (data.Identifier.ToLower().Contains("voltage"))
                    ini.Write("Text", data.Name + " %1 v", section);
                else if (data.Identifier.ToLower().Contains("frequency"))
                    ini.Write("Text", data.Name + " %1 Mhz", section);
                else
                    ini.Write("Text", data.Name + " %1", section);
                i++;
            }
        }

        private void InitializeComponent()
        {
            this.QueryText = new System.Windows.Forms.TextBox();
            this.SearchButton = new System.Windows.Forms.Button();
            this.Grid = new System.Windows.Forms.DataGridView();
            this.ColumnIdentifier = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.GenerateButton = new System.Windows.Forms.Button();
            this.NamespaceText = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
            this.TableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // QueryText
            // 
            this.QueryText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.QueryText.Location = new System.Drawing.Point(205, 3);
            this.QueryText.Name = "QueryText";
            this.QueryText.Size = new System.Drawing.Size(517, 22);
            this.QueryText.TabIndex = 0;
            this.QueryText.Text = "SELECT * FROM Sensor";
            // 
            // SearchButton
            // 
            this.SearchButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SearchButton.Location = new System.Drawing.Point(728, 3);
            this.SearchButton.Name = "SearchButton";
            this.SearchButton.Size = new System.Drawing.Size(101, 27);
            this.SearchButton.TabIndex = 1;
            this.SearchButton.Text = "Search";
            this.SearchButton.UseVisualStyleBackColor = true;
            this.SearchButton.Click += new System.EventHandler(this.SearchButtonClick);
            // 
            // Grid
            // 
            this.Grid.AllowUserToOrderColumns = true;
            this.Grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.Grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnIdentifier,
            this.ColumnName,
            this.ColumnValue});
            this.TableLayoutPanel.SetColumnSpan(this.Grid, 4);
            this.Grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Grid.Location = new System.Drawing.Point(3, 36);
            this.Grid.Name = "Grid";
            this.Grid.RowHeadersWidth = 49;
            this.Grid.RowTemplate.Height = 25;
            this.Grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.Grid.Size = new System.Drawing.Size(935, 611);
            this.Grid.TabIndex = 2;
            // 
            // ColumnIdentifier
            // 
            this.ColumnIdentifier.DataPropertyName = "Identifier";
            this.ColumnIdentifier.HeaderText = "Identifier";
            this.ColumnIdentifier.MinimumWidth = 6;
            this.ColumnIdentifier.Name = "ColumnIdentifier";
            // 
            // ColumnName
            // 
            this.ColumnName.DataPropertyName = "Name";
            this.ColumnName.HeaderText = "Name";
            this.ColumnName.MinimumWidth = 6;
            this.ColumnName.Name = "ColumnName";
            // 
            // ColumnValue
            // 
            this.ColumnValue.DataPropertyName = "Value";
            this.ColumnValue.HeaderText = "Value";
            this.ColumnValue.MinimumWidth = 6;
            this.ColumnValue.Name = "ColumnValue";
            // 
            // TableLayoutPanel
            // 
            this.TableLayoutPanel.ColumnCount = 4;
            this.TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 21.56261F));
            this.TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55.68128F));
            this.TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11.37806F));
            this.TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11.37806F));
            this.TableLayoutPanel.Controls.Add(this.Grid, 0, 1);
            this.TableLayoutPanel.Controls.Add(this.SearchButton, 2, 0);
            this.TableLayoutPanel.Controls.Add(this.GenerateButton, 3, 0);
            this.TableLayoutPanel.Controls.Add(this.QueryText, 1, 0);
            this.TableLayoutPanel.Controls.Add(this.NamespaceText, 0, 0);
            this.TableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.TableLayoutPanel.Name = "TableLayoutPanel";
            this.TableLayoutPanel.RowCount = 2;
            this.TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5.090312F));
            this.TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 94.90969F));
            this.TableLayoutPanel.Size = new System.Drawing.Size(941, 650);
            this.TableLayoutPanel.TabIndex = 3;
            // 
            // GenerateButton
            // 
            this.GenerateButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GenerateButton.Location = new System.Drawing.Point(835, 3);
            this.GenerateButton.Name = "GenerateButton";
            this.GenerateButton.Size = new System.Drawing.Size(103, 27);
            this.GenerateButton.TabIndex = 3;
            this.GenerateButton.Text = "Update";
            this.GenerateButton.UseVisualStyleBackColor = true;
            this.GenerateButton.Click += new System.EventHandler(this.GenerateClick);
            // 
            // NamespaceText
            // 
            this.NamespaceText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NamespaceText.Location = new System.Drawing.Point(3, 3);
            this.NamespaceText.Name = "NamespaceText";
            this.NamespaceText.Size = new System.Drawing.Size(196, 22);
            this.NamespaceText.TabIndex = 4;
            this.NamespaceText.Text = this.NamespaceText.Text;
            // 
            // ConfiguratorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(941, 650);
            this.Controls.Add(this.TableLayoutPanel);
            this.Name = "ConfiguratorForm";
            this.Text = "Configurator";
            this.Load += new System.EventHandler(this.ConfiguratorForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
            this.TableLayoutPanel.ResumeLayout(false);
            this.TableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        private void ConfiguratorForm_Load(object sender, EventArgs e)
        {
            try {
                NamespaceText.Text = NAMESPACE;
                ManagementObjectSearcher searchHW =
                    new ManagementObjectSearcher(NAMESPACE, QUERY_HW);

                foreach (ManagementObject queryObj in searchHW.Get()) {
                    m_hardware.Add(queryObj["Identifier"].ToString(), queryObj["Name"].ToString());
                }
            } catch (System.Management.ManagementException err) {
                NamespaceText.Text = "";
            }
        }
    }

    class Wrapper
    {
        private ManagementObject m_managementObject;
        private string m_name;

        public Wrapper(string name, ManagementObject managementObject)
        {
            m_managementObject = managementObject;
            m_name = name;
        }

        public string Name {
            set {
                if (value.Length > 0)
                    m_name = value;
            }
            get {
                return m_name;
            }
        }

        public string Identifier {
            get {
                return m_managementObject["Identifier"].ToString();
            }
        }

        public string Value {
            get {
                return m_managementObject["Value"].ToString();
            }
        }
    }
}
