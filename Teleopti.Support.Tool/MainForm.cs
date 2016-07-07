﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using Teleopti.Support.Tool.Controls;
using Teleopti.Support.Tool.DataLayer;
using Teleopti.Support.Tool.Properties;
using Teleopti.Support.Tool.Controls.DatabaseDeployment;


namespace Teleopti.Support.Tool
{
	public partial class MainForm : Form
	{
		private UserControl _activeControl;
		private Version _productVersion;
		private SettingsInRegistry _settingsInRegistry;
		private DBHelper _db;

		public MainForm()
		{
			InitializeComponent();
		}
		
		public void ShowPTracks()
		{
		  PTracks.Show();
		}

		public void HidePTracks()
		{
			PTracks.Hide();
		}

		//private void LLChangeDBConn_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		//{
		//    //this.Controls.Remove(activeControl);
		//    panelContent.Controls.Remove(activeControl);
		//    activeControl = new MainChangeDBSettings(this, _db);
		//    activeControl.Dock = DockStyle.Fill;
		//    PTracks.Hide();
		//    panelContent.Controls.Add(activeControl);
		//    //this.Controls.Add(activeControl);
		//}

		private void LLChangeDBConn_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			//this.Controls.Remove(activeControl);
			panelContent.Controls.Remove(_activeControl);
			_activeControl = new ChangeSettings(this);
			PTracks.Hide();
			panelContent.Controls.Add(_activeControl);
			_activeControl.Dock = DockStyle.Fill;
			//this.Controls.Add(activeControl);
		}
		private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			panelContent.Controls.Remove(_activeControl);
            _activeControl = new ManageDatabaseVersions(this, _productVersion, _db);
			_activeControl.Dock = DockStyle.Fill;
			PTracks.Hide();
			panelContent.Controls.Add(_activeControl);
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			_productVersion = new Version(Application.ProductVersion.Split('-')[0]);
#if (DEBUG)
			{
                //8.1.417.32156
                _productVersion = new Version(8, 1, 417, 32156);
                //_productVersion = new Version(8, 0, 397, 28396);
            }
#else
			{
				//_productVersion = new Version(8, 0, 397, 28396);
			}
#endif


			smoothLabelVersion.Text = _productVersion.ToString();
			_settingsInRegistry = new SettingsInRegistry();
			textBoxServerName.Text = _settingsInRegistry.SqlServerName;
			checkBoxUseWindowsAuthentication.Checked = _settingsInRegistry.UseWindowsAuthentication;
			textBoxUserName.Text = _settingsInRegistry.SqlUserName;

			SetVerifyButtonState();
			if (buttonVerifyCredentials.Enabled)
			{
				VerifyCredentials();
			}
		}

		private void SetVerifyButtonState()
		{
			smoothLabelConnected.Text = Resources.ClickVerifytoCheckYourCredentials;
			smoothLabelConnected.ForeColor = Color.Blue;

			if (!string.IsNullOrEmpty(textBoxServerName.Text))
			{
				if (checkBoxUseWindowsAuthentication.Checked)
				{
					buttonVerifyCredentials.Enabled = true;
					return;
				}
				if (!string.IsNullOrEmpty(textBoxUserName.Text))
				{
					buttonVerifyCredentials.Enabled = true;
					return;
				}
			}
			buttonVerifyCredentials.Enabled = false;
            SetDbAdminDependentLinkStates(false);
		}

		private void BClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void checkBoxUseWindowsAuthentication_CheckedChanged(object sender, EventArgs e)
		{
			_settingsInRegistry.UseWindowsAuthentication = checkBoxUseWindowsAuthentication.Checked;
			textBoxUserName.Enabled = !checkBoxUseWindowsAuthentication.Checked;
			textBoxPassword.Enabled = !checkBoxUseWindowsAuthentication.Checked;
			SetVerifyButtonState();
		}

		private void textBoxServerName_TextChanged(object sender, EventArgs e)
		{
			_settingsInRegistry.SqlServerName = textBoxServerName.Text;
		}

		private void buttonVerifyCredentials_Click(object sender, EventArgs e)
		{
			VerifyCredentials();
		}

		private void VerifyCredentials()
		{
			Cursor = Cursors.WaitCursor;

            SetDbAdminDependentLinkStates(false);
			smoothLabelConnected.ForeColor = Color.Blue;
			smoothLabelConnected.Text = Resources.Connecting;
			if (checkBoxUseWindowsAuthentication.Checked)
				_db = new DBHelper(textBoxServerName.Text, "master");
			else
				_db = new DBHelper(textBoxServerName.Text, textBoxUserName.Text, textBoxPassword.Text);
			List<string> databases = new List<string>();
			databases.Add("master");
			try
			{
				switch (_db.ConnectType(databases))
				{
					case DataLayer.Right.SA:
						smoothLabelConnected.ForeColor = Color.Green;
						smoothLabelConnected.Text = Resources.ConnectedAsSystemAdmin;
						_settingsInRegistry.SaveAll();
                        SetDbAdminDependentLinkStates(true);
						break;
					case DataLayer.Right.Limited:
						smoothLabelConnected.ForeColor = Color.Orange;
						smoothLabelConnected.Text = Resources.ConnectedWithLimitedAccess;
                        SetDbAdminDependentLinkStates(false);
						break;
					default:
						smoothLabelConnected.ForeColor = Color.Red;
						smoothLabelConnected.Text = Resources.NotConnected;
                        SetDbAdminDependentLinkStates(false);
						break;
				}
			}
			catch (SqlException ex)
			{
				smoothLabelConnected.ForeColor = Color.Red;
				smoothLabelConnected.Text = Resources.NotConnected + Environment.NewLine + ex.Message;
			}
			Cursor = Cursors.Default;
		}

        private void SetDbAdminDependentLinkStates(bool enabled)
        {
            linkManageDBVersions.Enabled = enabled;
            smoothLinkDeployDatabase.Enabled = enabled;
        }

		private void textBoxUserName_TextChanged(object sender, EventArgs e)
		{
			_settingsInRegistry.SqlServerName = textBoxServerName.Text;
			SetVerifyButtonState();
		}

		private void textBoxPassword_TextChanged(object sender, EventArgs e)
		{
			SetVerifyButtonState();
		}

		private void smoothLinkDeployDatabase_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			panelContent.Controls.Remove(_activeControl);
			_activeControl = new DatabaseDeployMain(this, _db, _settingsInRegistry) { Dock = DockStyle.Fill };
			PTracks.Hide();
			panelContent.Controls.Add(_activeControl);
		}
	}
}
