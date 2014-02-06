﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Teleopti.Support.Code.Tool;

namespace Teleopti.Support.Tool
{
    public partial class ChangeSettings : UserControl
    {
        private IList<SearchReplace> _settings;

        public ChangeSettings()
        {
            InitializeComponent();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            _settings = new SettingsFileManager(new SettingsReader()).GetReplaceList();
            dataGridView1.DataSource = _settings;
            dataGridView1.Columns[0].ReadOnly = true;
            dataGridView1.Columns[0].Width = Width / 2 - 37;
            dataGridView1.Columns[1].Width = Width / 2 -37;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            new SettingsFileManager(new SettingsReader()).SaveReplaceList(_settings);
        }

        private void buttonRefreshThem_Click(object sender, EventArgs e)
        {
	        var mode = "DEPLOY";
#if (DEBUG)
	        {
		        mode = "DEVELOP";
	        }
#endif
            runRefresher(mode);
        }

        private void runRefresher(string mode)
        {
            var fileMan = new SettingsFileManager(new SettingsReader());
            fileMan.SaveReplaceList(_settings);

            var refreshRunner = new RefreshConfigsRunner(fileMan, new RefreshConfigFile(new ConfigFileTagReplacer(),
                                                                                       new MachineKeyChecker()));
            refreshRunner.RefreshThem(mode);

	        if (mode.Equals("DEPLOY"))
	        {
				  string path = Directory.GetCurrentDirectory();

				  using (var proc = new System.Diagnostics.Process())
				  {
					  proc.StartInfo.FileName = path + "\\StartStopSystem\\ResetSystem.bat";
					  proc.StartInfo.Arguments = "ngt";
					  //proc.StartInfo.RedirectStandardError = true;
					  //proc.StartInfo.RedirectStandardOutput = true;
					  //proc.StartInfo.UseShellExecute = false;

					  proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
					  proc.StartInfo.CreateNoWindow = false;
					  proc.Start();
					  proc.WaitForExit();
				  }
	        }
        }
    }
}
