using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Forms;
using System.Xml.Linq;
using Microsoft.Win32;

namespace Teleopti.Support.Tool.Controls.DatabaseDeployment.Pages
{
	public partial class NHibernateSessionFilePage : SelectionPage
	{
		private readonly DatabaseDeploymentModel _model;

		public NHibernateSessionFilePage()
		{
			InitializeComponent();
		}

		public NHibernateSessionFilePage(DatabaseDeploymentModel model)
			: this()
		{
			_model = model;
			labelAppDB.Text = _model.SelectedAppDatabase.DatabasePath;
			labelAnalyticsDB.Text = _model.SelectedAnalyticsDatabase.DatabasePath;
			labelAggDB.Text = _model.SelectedAggDatabase.DatabasePath;
			var teleoptiCccSdkFolder = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Teleopti\TeleoptiCCC\InstallationSettings", "INSTALLDIR", @"C:\");
			if (teleoptiCccSdkFolder != @"C:\")
			{
				teleoptiCccSdkFolder = teleoptiCccSdkFolder + @"TeleoptiCCC\SDK\";
			}
			textBoxNhibFilePath.Text = teleoptiCccSdkFolder;
		}

		public override void SetData()
		{
			// ReSharper disable LocalizableElement
			labelAppDB.Text = _model.SelectedAppDatabase.DatabasePath + "    (" + _model.SelectedAppDatabase.DatabaseFromSourceType + ")";
			labelAnalyticsDB.Text = _model.SelectedAnalyticsDatabase.DatabasePath + "    (" + _model.SelectedAnalyticsDatabase.DatabaseFromSourceType + ")";
			labelAggDB.Text = _model.SelectedAggDatabase.DatabasePath + "    (" + _model.SelectedAggDatabase.DatabaseFromSourceType + ")";
			// ReSharper restore LocalizableElement
		}

		public override void GetData()
		{
			_model.NHibPath = textBoxNhibFilePath.Text;
			_model.SessionName = textBoxSessionName.Text;
		}

		public delegate void DeployDelegate();

		public event DeployDelegate Deploy;

		private void buttonDeploy_Click(object sender, EventArgs e)
		{
			GetData();
			Deploy();
			return;
		}


		private void buttonBrowseNHibLocation_Click(object sender, EventArgs e)
		{
			folderBrowserDialogNhib.SelectedPath = textBoxNhibFilePath.Text;
			if (folderBrowserDialogNhib.ShowDialog(this) != DialogResult.OK)
			{
				return;
			}
			textBoxNhibFilePath.Text = folderBrowserDialogNhib.SelectedPath;
		}

		private void nHibernateSessionFilePage_Load(object sender, EventArgs e)
		{
			if (textBoxSessionName.Text == "")
			{
				if (_model.SelectedAppDatabase.DatabaseFromSourceType == DatabaseSourceType.FromArchive)
				{
					var path = _model.ZipFilePath;
					textBoxSessionName.Text =
						path.Substring(path.LastIndexOf("\\", StringComparison.Ordinal) + 1)
							.Replace(".zip", "")
							.Replace(".7z", "");
				}
				else
				{
					textBoxSessionName.Text = _model.SelectedAppDatabase.DatabaseName;
				}
			}

		}
	}
}