﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Support.Library.Config;
using Teleopti.Support.Tool.Tool;

namespace Teleopti.Support.Tool.Controls
{
	public partial class ChangeSettings : UserControl
	{
		private readonly MainForm _mainForm;
		private IList<SearchReplace> _settings;

		public ChangeSettings()
		{
			InitializeComponent();
		}

		public ChangeSettings(MainForm mainForm)
		{
			InitializeComponent();
			_mainForm = mainForm;
		}

		private void SettingsForm_Load(object sender, EventArgs e)
		{
			_settings = new SettingsFileManager()
				.ReadFile()
				.ForDisplay()
				.ToList();
			dataGridView1.DataSource = _settings;
			dataGridView1.Columns[0].ReadOnly = true;
		}

		private void buttonSave_Click(object sender, EventArgs e)
		{
			new SettingsFileManager().SaveFile(_settings);
		}

		private void buttonRefreshThem_Click(object sender, EventArgs e)
		{

            var mode = "DeployConfigFiles.txt";
#if (DEBUG)
			{
				mode = "ConfigFiles.txt";
			}
#endif
			runRefresher(mode);
		}

		private void runRefresher(string mode)
		{
			new SettingsFileManager().SaveFile(_settings);

			var refreshRunner = new RefreshConfigsRunner(new RefreshConfigFile());
			refreshRunner.Execute(new ModeFile(mode));

			if (mode.Equals("DEPLOY"))
			{
				var path = Directory.GetCurrentDirectory();
				var theFile = path + "\\StartStopSystem\\ResetSystem.bat";
				try
				{
					using (var proc = new System.Diagnostics.Process())
					{
						proc.StartInfo.FileName = theFile;
						proc.StartInfo.Arguments = "ngt";
						proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
						proc.StartInfo.CreateNoWindow = false;
						proc.Start();
						proc.WaitForExit();
					}
				}
				catch (Exception exception)
				{
					MessageBox.Show(exception.Message + Environment.NewLine + theFile, "Some Error occurred");
				}

			}
		}

		private void BBack_Click(object sender, EventArgs e)
		{
			_mainForm.ShowPTracks();
			Hide();

		}

		private void dataGridView1_Resize(object sender, EventArgs e)
		{
			//dataGridView1.Columns[0].Width = Width / 2 - 37;
			//dataGridView1.Columns[1].Width = Width / 2 - 37;

		}
	}
}
