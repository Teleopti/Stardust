using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using Teleopti.Support.Tool.DataLayer;

namespace Teleopti.Support.Tool.Controls
{
	public partial class ManageDatabaseVersions : UserControl
	{
		private const int ImageIndexDatabaseVersionOk = 0;
		private const int ImageIndexDatabaseVersionNotOk = 1;
		private const int ImageIndexDatabaseConnecting = 2;
		private const int ImageIndexDatabaseNotConnected = 3;
		private const String SPACE = " ";
		private readonly MainForm _mainForm;
		private readonly Version _currentVersion;
		private readonly DBHelper _dbHelper;
		private string _teleoptiCccBaseInstallFolder;
		private string _lastSelectedItemText;
		private string _dbManagerHomeFolder;
		private IList<NotepadStarter> _notepadStarters;


		public ManageDatabaseVersions(MainForm mainForm, Version currentVersion, DBHelper dbHelper)
		{
			_notepadStarters = new List<NotepadStarter>();
			_mainForm = mainForm;
			_currentVersion = currentVersion;
			_dbHelper = dbHelper;
			InitializeComponent();
		}

		private void ManageDatabaseVersions_Load(object sender, EventArgs e)
		{
			smoothLabelCurrentVersion.Text = _currentVersion.ToString();
			_teleoptiCccBaseInstallFolder = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Teleopti\TeleoptiCCC\InstallationSettings", "INSTALLDIR", @"C:\");
			textBoxNHibFolder.Text = Directory.GetCurrentDirectory() + @"\..\ConfigurationFiles\";
			RefreshDatabaseList();
			_dbManagerHomeFolder = _teleoptiCccBaseInstallFolder + @"DatabaseInstaller\";

#if (DEBUG)
			{
				_dbManagerHomeFolder = @"C:\data\main_clone\Database\";
			}
#endif

		}

		private void buttonBack_Click(object sender, EventArgs e)
		{
			SetButtonStates(true);
			if (listViewDatabases.Visible)
			{
				_mainForm.ShowPTracks();
				Hide();
			}
			else
			{
				listViewDatabases.Visible = true;
				textBoxOutput.Visible = false;
				RefreshDatabaseList();
			}
		}

		private void buttonBrowse_Click(object sender, EventArgs e)
		{
			folderBrowserDialogNHib.SelectedPath = textBoxNHibFolder.Text;
			folderBrowserDialogNHib.ShowDialog();
			if (!textBoxNHibFolder.Text.Equals(folderBrowserDialogNHib.SelectedPath))
			{
				textBoxNHibFolder.Text = folderBrowserDialogNHib.SelectedPath;
				RefreshDatabaseList();
			}
		}

		private void RefreshDatabaseList()
		{
			ThreadSafeControlDelegation.SetCursor(Cursors.WaitCursor, buttonRefresh);
			ThreadSafeControlDelegation.SetEnabled(false, buttonRefresh);

			listViewDatabases.BeginUpdate();
			listViewDatabases.Items.Clear();
			IList<Nhib> nhibs = new List<Nhib>();
			try
			{
				nhibs = XmlHandler.GetNhibSettings(textBoxNHibFolder.Text).ToList();
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
				listViewDatabases.EndUpdate();
			}

			foreach (Nhib nhib in nhibs)
			{
				string groupCaption = nhib.FactoryName + " (" + nhib.CccDataSource.ServerName + ")";
				ListViewGroup listViewGroup = listViewDatabases.Groups.Add(nhib.FactoryName, groupCaption);
				listViewGroup.Tag = nhib;
				ListViewItem[] listViewItems = CreateDatabaseListViewItems(nhib, listViewGroup);
				listViewDatabases.Items.AddRange(listViewItems);
			}
			listViewDatabases.EndUpdate();
			if (!string.IsNullOrEmpty(_lastSelectedItemText))
			{
				ListViewItem preselectedListViewItem = listViewDatabases.FindItemWithText(_lastSelectedItemText);
				if (preselectedListViewItem != null)
				{
					preselectedListViewItem.EnsureVisible();
					preselectedListViewItem.Selected = true;
				}
			}
			while (backgroundWorkerConnectionAndVersion.IsBusy)
			{
				if (!backgroundWorkerConnectionAndVersion.CancellationPending)
				{
					backgroundWorkerConnectionAndVersion.CancelAsync();
				}
				Application.DoEvents();
			}
			backgroundWorkerConnectionAndVersion.RunWorkerAsync();
		}


		private void backgroundWorkerConnectionAndVersion_DoWork(object sender, DoWorkEventArgs e)
		{
			foreach (ListViewItem listViewItem in GetListViewItems(listViewDatabases))
			{
				if (backgroundWorkerConnectionAndVersion.CancellationPending)
				{
					return;
				}
				NHibDataSource nHibDataSource = (NHibDataSource)listViewItem.Tag;
				SetListviewIcon(nHibDataSource, listViewItem);
			}

			ThreadSafeControlDelegation.SetEnabled(true, buttonRefresh);
			ThreadSafeControlDelegation.SetCursor(Cursors.Default, buttonRefresh);
			Application.DoEvents();
		}

		private ListViewItem[] CreateDatabaseListViewItems(Nhib nhib, ListViewGroup listViewGroup)
		{
			var items = new Collection<ListViewItem>();
			ListViewItem listViewItem;
			NHibDataSource nHibDataSource = nhib.CccDataSource;
			if (!listViewDatabases.Items.ContainsKey(nHibDataSource.Id))
			{
				listViewItem = CreateDatasourceListViewItem(listViewGroup, nHibDataSource);
				items.Add(listViewItem);
				nHibDataSource = nhib.AnalyticsDataSource;
				listViewItem.SubItems.Add(new ListViewItem.ListViewSubItem(listViewItem, nHibDataSource.DatabaseName) { Tag = nHibDataSource });
				listViewItem.SubItems.Add(new ListViewItem.ListViewSubItem(listViewItem, nHibDataSource.Version));
			}
			return items.ToArray();
		}

		private ListViewItem CreateDatasourceListViewItem(ListViewGroup listViewGroup, NHibDataSource nHibDataSource)
		{
			ListViewItem listViewItem;
			listViewItem = CreateDatabaseListViewItem(nHibDataSource);
			listViewItem.Group = listViewGroup;
			return listViewItem;
		}

		private ListViewItem CreateDatabaseListViewItem(NHibDataSource nHibDataSource)
		{
			ListViewItem listViewItem = new ListViewItem(nHibDataSource.DatabaseName);
			listViewItem.Tag = nHibDataSource;
			listViewItem.Name = nHibDataSource.Id;
			listViewItem.ImageIndex = ImageIndexDatabaseConnecting;
			if (!IsOnSameServer(nHibDataSource))
			{
				listViewItem.ForeColor = Color.FromKnownColor(KnownColor.InactiveCaptionText);
				listViewItem.ToolTipText = "Not on " + _dbHelper.ServerName;
			}

			ListViewItem.ListViewSubItem listViewSubItem;
			listViewSubItem = new ListViewItem.ListViewSubItem(listViewItem, nHibDataSource.Version);

			listViewItem.SubItems.Add(listViewSubItem);
			return listViewItem;
		}

		private bool IsOnSameServer(NHibDataSource nHibDataSource)
		{
			return _dbHelper.ServerName.Equals(nHibDataSource.ServerName, StringComparison.InvariantCultureIgnoreCase);
		}

		private void buttonRefresh_Click(object sender, EventArgs e)
		{
			RefreshDatabaseList();
		}

		private void buttonUpdate_Click(object sender, EventArgs e)
		{
			_mainForm.Cursor = Cursors.WaitCursor;
			SetButtonStates(false);
			listViewDatabases.Visible = false;
			textBoxOutput.Clear();
			textBoxOutput.Visible = true;
			execute();
			buttonBack.Enabled = true;
			_mainForm.Cursor = Cursors.Default;
		}

		private void SetButtonStates(bool state)
		{
			textBoxNHibFolder.Enabled = state;
			buttonBrowse.Enabled = state;
			buttonBack.Enabled = state;
			buttonRefresh.Enabled = state;
			buttonUpdate.Enabled = state;
		}


		private void execute()
		{
			try
			{
				foreach (ListViewItem listViewItem in listViewDatabases.SelectedItems)
				{
					var nHibDataSource = (NHibDataSource)listViewItem.Tag;
					var nHibAnalyticsDataSource = (NHibDataSource)listViewItem.SubItems[1].Tag;
					string databaseTypeString = listViewItem.SubItems[1].Text;

					var processStartInfo = CreateProcessStartInfoForDBManager(nHibDataSource, databaseTypeString);
					RunProcess(processStartInfo);

					processStartInfo = CreateProcessStartInfoForSecurity(nHibDataSource, nHibAnalyticsDataSource);
					RunProcess(processStartInfo);
				}

				AppendText("Database update finished successfully to version " + _currentVersion);
			}
			catch (SqlException e)
			{
				AppendText(e.Message);
			}
			catch (ApplicationException e)
			{
				AppendText(e.Message);
			}

		}

		private ProcessStartInfo CreateProcessStartInfoForSecurity(NHibDataSource nHibDataSourceApp, NHibDataSource nHibDataSourceAnalytics)
		{
			var stringBuilder = new StringBuilder();
			var sqlConnectionStringBuilderAdmin = new SqlConnectionStringBuilder(_dbHelper.ConnectionString);
			var workingDirectory = _teleoptiCccBaseInstallFolder + @"DatabaseInstaller\Enrypted\";

			var command = workingDirectory + @"Teleopti.Support.Security.exe";
			var processStartInfo = new ProcessStartInfo(command);
			stringBuilder.Append(SPACE);
			stringBuilder.Append(@"-DS");
			stringBuilder.Append(nHibDataSourceApp.ServerName + SPACE);
			stringBuilder.Append(getLogonString(sqlConnectionStringBuilderAdmin));
			stringBuilder.Append(@"-AP");
			stringBuilder.Append(nHibDataSourceApp.DatabaseName + SPACE);
			stringBuilder.Append(@"-AN");
			stringBuilder.Append(nHibDataSourceAnalytics.DatabaseName + SPACE);
			stringBuilder.Append(@"-CD");
			string aggDBName = _dbHelper.GetAggDatabaseName(nHibDataSourceApp.DatabaseName);
			stringBuilder.Append(aggDBName + SPACE);
			processStartInfo.Arguments = stringBuilder.ToString();

			processStartInfo.WorkingDirectory = workingDirectory;

			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.RedirectStandardError = true;
			processStartInfo.UseShellExecute = false;
			processStartInfo.CreateNoWindow = true;
			return processStartInfo;
		}

		private static string getLogonString(SqlConnectionStringBuilder sqlConnectionStringBuilder)
		{
			if (sqlConnectionStringBuilder.IntegratedSecurity)
			{
				return "-EE ";
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(@"-DU");
			stringBuilder.Append(sqlConnectionStringBuilder.UserID + SPACE);
			stringBuilder.Append(@"-DP");
			stringBuilder.Append(sqlConnectionStringBuilder.Password + SPACE);
			return stringBuilder.ToString();
		}


		private void RunProcess(ProcessStartInfo processStartInfo)
		{
			Process process = new Process();
			process.EnableRaisingEvents = true;
			process.OutputDataReceived += new DataReceivedEventHandler(ProcOnOutputDataReceived);
			process.ErrorDataReceived += new DataReceivedEventHandler(ProcOnErrorDataReceived);
			process.Exited += new EventHandler(ProcOnExited);
			process.StartInfo = processStartInfo;
			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
			while (!process.HasExited)
			{
				Application.DoEvents();
			}
			int exitCode = process.ExitCode;
			process.Dispose();

			if (exitCode != 0)
			{
				throw new ApplicationException(processStartInfo.FileName + " returned exit code: " + exitCode);
			}
		}

		//          1 - alltid
		//          DBManager.exe -S%MyServerInstance% -E -D%DATABASE% -O%DATABASETYPE% -E -T
		private ProcessStartInfo CreateProcessStartInfoForDBManager(NHibDataSource nHibDataSource, string databaseTypeString)
		{
			StringBuilder stringBuilder = new StringBuilder();
			SqlConnectionStringBuilder sqlConnectionStringBuilderAdmin = new SqlConnectionStringBuilder(_dbHelper.ConnectionString);
			SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(nHibDataSource.ConnectionString);
			string workingDirectory = _teleoptiCccBaseInstallFolder + @"DatabaseInstaller\";

			string command = workingDirectory + @"DBManager.exe";
			ProcessStartInfo processStartInfo = new ProcessStartInfo(command);
			stringBuilder.Append(SPACE);
			stringBuilder.Append(@"-S");
			stringBuilder.Append(sqlConnectionStringBuilder.DataSource + SPACE);
			stringBuilder.Append(getLogonStringForDBManager(sqlConnectionStringBuilderAdmin));
			stringBuilder.Append(@"-D");
			stringBuilder.Append(nHibDataSource.DatabaseName + SPACE);
			stringBuilder.Append(@"-O");
			stringBuilder.Append(getCccDbType(databaseTypeString) + SPACE);
			stringBuilder.Append(@"-T" + SPACE);

			stringBuilder.Append(getLogonStringForXXX(sqlConnectionStringBuilder));
			//-R -L%SQLAppLogin%:%SQLAppPwd%
			// nHibDataSource.ConnectionString

			processStartInfo.Arguments = stringBuilder.ToString();

			processStartInfo.WorkingDirectory = workingDirectory;
			//This means that it will be redirected to the Process.StandardOutput StreamReader.
			processStartInfo.RedirectStandardOutput = true;


			//This means that it will be redirected to the Process.StandardError StreamReader. (same as StdOutput)
			processStartInfo.RedirectStandardError = true;

			processStartInfo.UseShellExecute = false;
			// Do not create the black window.
			processStartInfo.CreateNoWindow = true;

			return processStartInfo;
		}

		//-R -L%SQLAppLogin%:%SQLAppPwd%
		private static string getLogonStringForXXX(SqlConnectionStringBuilder sqlConnectionStringBuilder)
		{
			StringBuilder stringBuilder = new StringBuilder();


			if (sqlConnectionStringBuilder.IntegratedSecurity)
			{
				// if we are running under windows, we must provide an existing windows group from SQL SERVER.
				// you can use this SQL Query to see what windows group are available 
				// SET NOCOUNT ON;SELECT ROW_NUMBER() OVER (ORDER BY name) AS ComboOrder, name from sys.syslogins where isntgroup = 1 and name <> 'NT SERVICE\MSSQLSERVER' and name <> 'NT SERVICE\SQLSERVERAGENT'
				// stringBuilder.Append(@"-W");
			}
			else
			{
				stringBuilder.Append("-R" + SPACE);
				stringBuilder.Append(@"-L");
				stringBuilder.Append(sqlConnectionStringBuilder.UserID);
				stringBuilder.Append(@":");
				stringBuilder.Append(sqlConnectionStringBuilder.Password);
			}
			return stringBuilder.ToString();
		}

		private static string getLogonStringForDBManager(SqlConnectionStringBuilder sqlConnectionStringBuilder)
		{
			if (sqlConnectionStringBuilder.IntegratedSecurity)
			{
				return "-E ";
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(@"-U");
			stringBuilder.Append(sqlConnectionStringBuilder.UserID + SPACE);
			stringBuilder.Append(@"-P");
			stringBuilder.Append(sqlConnectionStringBuilder.Password + SPACE);
			return stringBuilder.ToString();
		}

		private static string getCccDbType(string databaseTypeString)
		{

			switch (databaseTypeString)
			{
				case Nhib.ApplicationDatabaseTextConstant:
					return "TeleoptiCCC7";
				case Nhib.AnalyticsDatabaseTextConstant:
					return "TeleoptiAnalytics";
				case Nhib.AggregationDatabaseTextConstant:
					return "TeleoptiCCCAgg";
			}
			return "";
		}

		private void AppendText(string text)
		{
			if (InvokeRequired)
			{
				this.BeginInvoke(new Action<string>(AppendText), new object[] { text });
				return;
			}


			textBoxOutput.Text += text;
			textBoxOutput.Text += Environment.NewLine;
			textBoxOutput.Select(textBoxOutput.Text.Length - 1, 0);
			textBoxOutput.ScrollToCaret();
			//textBoxOutput.AppendText(@"\n");
		}

		private void ProcOnExited(object sender, EventArgs eventArgs)
		{
			//buttonBack.Invoke(new EnableBackDelegate(EnableBack));
		}

		private void ProcOnErrorDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
		{
			AppendText(dataReceivedEventArgs.Data);
		}

		private void ProcOnOutputDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
		{
			AppendText(dataReceivedEventArgs.Data);
		}

		private void listViewDatabases_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			buttonUpdate.Enabled = false;

			NHibDataSource clickedNHibDataSource;

			if (e.Item.Selected)
			{
				clickedNHibDataSource = (NHibDataSource)e.Item.Tag;
				if (!IsOnSameServer(clickedNHibDataSource))
				{
					e.Item.Selected = false;
				}
			}

			foreach (ListViewItem listViewItem in listViewDatabases.SelectedItems)
			{
				clickedNHibDataSource = (NHibDataSource)listViewItem.Tag;
				if (IsOnSameServer(clickedNHibDataSource))
				{
					string clickedVersion = listViewItem.SubItems[1].Text;
					string splittedVersion = clickedVersion.Split('-')[0];
					if (!string.IsNullOrEmpty(splittedVersion))
					{
						Version dbVersion;
						if (Version.TryParse(splittedVersion, out dbVersion))
						{
							if (dbVersion <= _currentVersion)
							{
								buttonUpdate.Enabled = true;
								break;
							}
						}
					}
				}
				else
				{
					listViewItem.Selected = false;
				}
			}
			_lastSelectedItemText = e.Item.Text;
		}

		private void listViewDatabases_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private delegate void ChangeListViewIconDelegate(int newImageIndex, int listViewItemIndex);

		private void ChangeListViewIcon(int newImageIndex, int listViewItemIndex)
		{
			ListViewItem listViewItem = listViewDatabases.Items[listViewItemIndex];
			NHibDataSource nHibDataSource = (NHibDataSource)listViewItem.Tag;
			listViewItem.ImageIndex = newImageIndex;
			switch (newImageIndex)
			{
				case ImageIndexDatabaseNotConnected:
					listViewItem.SubItems[1].Text = NHibDataSource.NotConnected;
					break;
				case ImageIndexDatabaseVersionNotOk:
				case ImageIndexDatabaseVersionOk:
					listViewItem.SubItems[1].Text = nHibDataSource.Version;
					listViewItem.SubItems[3].Text = ((NHibDataSource)listViewItem.SubItems[2].Tag).Version;
					if (nHibDataSource.CccDatabaseType == Nhib.AnalyticsDatabaseTextConstant)
					{
						SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(_dbHelper.ConnectionString);
						builder.InitialCatalog = nHibDataSource.DatabaseName;
						var dbHelper = new DBHelper(builder.ConnectionString);
						string aggDbName = dbHelper.GetAggDatabaseName();
						var builderForAgg = new SqlConnectionStringBuilder(nHibDataSource.ConnectionString);
						builderForAgg.InitialCatalog = aggDbName;
						var aggNHibDataSource = new NHibDataSource(nHibDataSource.FactoryName, builderForAgg.ConnectionString, Nhib.AggregationDatabaseTextConstant);

						ListViewItem aggListViewItem = CreateDatasourceListViewItem(listViewItem.Group, aggNHibDataSource);
						listViewDatabases.Items.Add(aggListViewItem);

						SetListviewIcon(aggNHibDataSource, aggListViewItem);
					}
					break;
			}
		}

		private delegate ListView.ListViewItemCollection GetItemsDelegate(ListView lstview);
		private ListView.ListViewItemCollection GetListViewItems(ListView listView)
		{
			ListView.ListViewItemCollection temp = new ListView.ListViewItemCollection(new ListView());
			if (!listView.InvokeRequired)
			{
				foreach (ListViewItem item in listView.Items)
					temp.Add((ListViewItem)item.Clone());
				return temp;
			}
			else
				return (ListView.ListViewItemCollection)this.Invoke(new GetItemsDelegate(GetListViewItems), new object[] { listView });
		}



		private int getLatestScriptFileBuildNumber(NHibDataSource nHibDataSource)
		{
			string dbTypeText = getCccDbType(nHibDataSource.CccDatabaseType);
			string scriptFileFolder = _dbManagerHomeFolder + dbTypeText + @"\Releases\";
			DirectoryInfo releasesDirectoryInfo = new DirectoryInfo(scriptFileFolder);
			FileInfo[] files = releasesDirectoryInfo.GetFiles("0*.sql", SearchOption.TopDirectoryOnly);
			List<string> listOfFileNames = new List<string>();
			foreach (var fileInfo in files)
			{
				listOfFileNames.Add(fileInfo.Name);
			}
			listOfFileNames.Sort();
			string lastScriptFile = listOfFileNames.Last<string>();
			lastScriptFile = lastScriptFile.Replace(".sql", string.Empty);
			return Convert.ToInt32(lastScriptFile);
		}


		private void SetListviewIcon(NHibDataSource nHibDataSource, ListViewItem listViewItem)
		{
			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(_dbHelper.ConnectionString);
			builder.InitialCatalog = nHibDataSource.DatabaseName;
			int latestScriptFileBuildNumber = getLatestScriptFileBuildNumber(nHibDataSource);
			DBHelper dbHelper = new DBHelper(builder.ConnectionString);
			if (dbHelper.TestConnection())
			{
				int databaseBuildNumber = dbHelper.GetDatabaseBuildNumber();
				nHibDataSource.Version = dbHelper.GetDatabaseVersion();

				string[] s = nHibDataSource.Version.Split('-');
				Version dbVersion = new Version(s[0]);

				var dataSource = (NHibDataSource)listViewItem.SubItems[2].Tag;
				builder.InitialCatalog = dataSource.DatabaseName;
				dbHelper = new DBHelper(builder.ConnectionString);
				if (dbHelper.TestConnection())
				{
					dataSource.Version = dbHelper.GetDatabaseVersion();
				}
				CallSetListViewIcon(databaseBuildNumber < latestScriptFileBuildNumber ? ImageIndexDatabaseVersionNotOk : ImageIndexDatabaseVersionOk,
				 listViewItem.Index);
			}
			else
			{
				CallSetListViewIcon(ImageIndexDatabaseNotConnected, listViewItem.Index);
			}



		}

		private void CallSetListViewIcon(int imageIndex, int listviewItemIndex)
		{
			if (listViewDatabases.InvokeRequired)
			{
				if (backgroundWorkerConnectionAndVersion.CancellationPending)
				{
					return;
				}
				listViewDatabases.Invoke(
					 new ChangeListViewIconDelegate((ChangeListViewIcon)),
					 new object[] { imageIndex, listviewItemIndex });
			}
			else
			{
				ChangeListViewIcon(imageIndex, listviewItemIndex);
			}
		}

		private void toolStripMenuItemEdit_Click(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(_lastSelectedItemText))
			{
				ListViewItem preselectedListViewItem = listViewDatabases.FindItemWithText(_lastSelectedItemText);
				if (preselectedListViewItem != null)
				{
					Nhib nhib = (Nhib)preselectedListViewItem.Group.Tag;
					NotepadStarter notepadStarter = new NotepadStarter(nhib.FileName);
					_notepadStarters.Add(notepadStarter);
					notepadStarter.NotepadExited += new NotepadStarter.NotepadExitedDelegate(notepadStarter_NotepadExited);
					notepadStarter.Run();
				}
			}

		}

		void notepadStarter_NotepadExited(object sender, EventArgs e)
		{
			NotepadStarter notepadStarter = (NotepadStarter)sender;
			_notepadStarters.Remove(notepadStarter);
		}

		private void contextMenuStripDatabases_Opening(object sender, CancelEventArgs e)
		{
			if (listViewDatabases.SelectedItems.Count.Equals(1))
			{
				var nHibDataSource = listViewDatabases.SelectedItems[0].Tag as NHibDataSource;
				if (nHibDataSource != null && nHibDataSource.CccDatabaseType.Equals(Nhib.AggregationDatabaseTextConstant))
				{
					toolStripMenuItemEdit.Enabled = false;
					changeNameToolStripMenuItem.Enabled = true;
				}
				else
				{
					toolStripMenuItemEdit.Enabled = true;
					changeNameToolStripMenuItem.Enabled = false;
				}
			}
		}

		private void changeNameToolStripMenuItem_Click(object sender, EventArgs e)
		{
			listViewDatabases.LabelEdit = true;
			listViewDatabases.SelectedItems[0].BeginEdit();
		}

		private void listViewDatabases_AfterLabelEdit(object sender, LabelEditEventArgs e)
		{
			if (e.Label.Equals("")) e.CancelEdit = true;
			var nHibDataSource = listViewDatabases.SelectedItems[0].Tag as NHibDataSource;
			if (nHibDataSource != null && nHibDataSource.CccDatabaseType.Equals(Nhib.AggregationDatabaseTextConstant))
			{
				var aggListViewItem = listViewDatabases.SelectedItems[0];
				var builderForAgg = new SqlConnectionStringBuilder(nHibDataSource.ConnectionString) { InitialCatalog = e.Label };
				var aggNHibDataSource = new NHibDataSource(nHibDataSource.FactoryName, builderForAgg.ConnectionString,
					Nhib.AggregationDatabaseTextConstant);
				aggListViewItem.Tag = aggNHibDataSource;
				aggListViewItem.ToolTipText = e.Label;
				SetListviewIcon(aggNHibDataSource, aggListViewItem);

				var analytic = aggListViewItem.Group.Items[aggListViewItem.Group.Items.IndexOf(aggListViewItem) - 1];

				var nHibDataSourceAnalytics = analytic.Tag as NHibDataSource;

				if (nHibDataSourceAnalytics != null
					&& nHibDataSourceAnalytics.CccDatabaseType == Nhib.AnalyticsDatabaseTextConstant
					&& !aggListViewItem.SubItems[2].Text.Equals(NHibDataSource.NotConnected))
					_dbHelper.UpdateCrossRef(aggNHibDataSource.DatabaseName, nHibDataSourceAnalytics.DatabaseName);
			}
			else
			{
				e.CancelEdit = true;
			}
		}
	}
}
