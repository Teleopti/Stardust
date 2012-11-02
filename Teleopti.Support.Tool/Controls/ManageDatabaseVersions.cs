﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Globalization;
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
        private const string DOUBLEQUOTE = @"""";
        private const String SPACE = " ";
        private readonly MainForm _mainForm;
        private readonly Version _currentVersion;
        private string _teleoptiCccBaseInstallFolder;
        private string _lastSelectedItemText;
        private IList<NotepadStarter> _notepadStarters;


        public ManageDatabaseVersions(MainForm mainForm, Version currentVersion)
        {
            _notepadStarters = new List<NotepadStarter>();
            _mainForm = mainForm;
            _currentVersion = currentVersion;
            InitializeComponent();
        }

        private void ManageDatabaseVersions_Load(object sender, EventArgs e)
        {
            smoothLabelCurrentVersion.Text = _currentVersion.ToString();
            _teleoptiCccBaseInstallFolder = (string) Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Teleopti\TeleoptiCCC\InstallationSettings", "INSTALLDIR", @"C:\");
            textBoxNHibFolder.Text = _teleoptiCccBaseInstallFolder + @"TeleoptiCCC\SDK\";
            RefreshDatabaseList();
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
            if (backgroundWorkerConnectionAndVersion.IsBusy)
            {
                backgroundWorkerConnectionAndVersion.CancelAsync();
            }

            listViewDatabases.BeginUpdate();
            listViewDatabases.Items.Clear();
            IList<Nhib> nhibs = XmlHandler.GetNhibSettings(textBoxNHibFolder.Text).ToList();
            foreach (Nhib nhib in nhibs)
            {
                
                string groupCaption = nhib.FactoryName + " ("+ nhib.CccDataSource.ServerName + ")";
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
            backgroundWorkerConnectionAndVersion.RunWorkerAsync();
        }


        private void backgroundWorkerConnectionAndVersion_DoWork(object sender, DoWorkEventArgs e)
        {
            foreach (ListViewItem listViewItem in GetListViewItems(listViewDatabases))
            {
                NHibDataSource nHibDataSource = (NHibDataSource)listViewItem.Tag;
                SetListviewIcon(nHibDataSource, listViewItem);
            }

            ThreadSafeControlDelegation.SetEnabled(true, buttonRefresh);
            //ThreadSafeControlDelegation.SetEnabled(true, buttonBack);
            ThreadSafeControlDelegation.SetCursor(Cursors.Default, buttonRefresh);
        }

        private ListViewItem[] CreateDatabaseListViewItems(Nhib nhib, ListViewGroup listViewGroup)
        {
            Collection<ListViewItem> items = new Collection<ListViewItem>();
            ListViewItem listViewItem;
            NHibDataSource nHibDataSource = nhib.CccDataSource;
            if (!listViewDatabases.Items.ContainsKey(nHibDataSource.Id))
            {
                listViewItem = CreateDatasourceListViewItem(listViewGroup, nHibDataSource);
                items.Add(listViewItem);
            }
            nHibDataSource = nhib.AnalyticsDataSource;
            if (!listViewDatabases.Items.ContainsKey(nHibDataSource.Id))
            {
                listViewItem = CreateDatasourceListViewItem(listViewGroup, nHibDataSource);
                items.Add(listViewItem);
            }
            return items.ToArray();
        }

        private static ListViewItem CreateDatasourceListViewItem(ListViewGroup listViewGroup, NHibDataSource nHibDataSource)
        {
            ListViewItem listViewItem;
            listViewItem = CreateDatabaseListViewItem(nHibDataSource);
            listViewItem.Group = listViewGroup;
            return listViewItem;
        }

        private static ListViewItem CreateDatabaseListViewItem(NHibDataSource nHibDataSource)
        {
            ListViewItem listViewItem = new ListViewItem(nHibDataSource.DatabaseName);
            listViewItem.Name = nHibDataSource.Id;
            listViewItem.Tag = nHibDataSource;
            listViewItem.ImageIndex = ImageIndexDatabaseConnecting;
            ListViewItem.ListViewSubItem listViewSubItem;
            listViewSubItem = new ListViewItem.ListViewSubItem(listViewItem, nHibDataSource.CccDatabaseType);
            listViewItem.SubItems.Add(listViewSubItem);
            listViewSubItem = new ListViewItem.ListViewSubItem(listViewItem, nHibDataSource.Version);
            listViewItem.SubItems.Add(listViewSubItem);
            return listViewItem;
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

        delegate void EnableBackDelegate();

        private void EnableBack()
        {
            buttonBack.Enabled = true;
        }



        private void execute()
        {
            try
            {
                foreach (ListViewItem listViewItem in listViewDatabases.SelectedItems)
                {
                    NHibDataSource nHibDataSource = (NHibDataSource) listViewItem.Tag;
                    string databaseTypeString = listViewItem.SubItems[1].Text;

                    ProcessStartInfo processStartInfo;
                    processStartInfo = CreateProcessStartInfoForDBManager(nHibDataSource, databaseTypeString);
                    RunProcess(processStartInfo);

                    if (databaseTypeString == Nhib.ApplicationDatabaseTextConstant)
                    {
                        processStartInfo = createProcessStartInfoForApplicationSecurity(nHibDataSource);
                        RunProcess(processStartInfo);
                    }

                    if (databaseTypeString == Nhib.AnalyticsDatabaseTextConstant)
                    {
                        processStartInfo = CreateProcessStartInfoForAnalyticsSecurity(nHibDataSource);
                        RunProcess(processStartInfo);
                    }
                }

                AppendText("Database update finished successfully to version " + _currentVersion);
            }
            catch (ApplicationException e)
            {
                AppendText(e.Message);
            }
          
        }


//                  3 – If TeleoptiAnalytics DbType
//                  Teleopti.Support.Security.exe -DS%MyServerInstance% -DD%DATABASE% -CD%TeleoptiCCCAgg% -EE    
        private ProcessStartInfo CreateProcessStartInfoForAnalyticsSecurity(NHibDataSource nHibDataSource)
        {
            StringBuilder stringBuilder = new StringBuilder();
            SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(nHibDataSource.ConnectionString);
            string workingDirectory = _teleoptiCccBaseInstallFolder + @"DatabaseInstaller\Enrypted\";

            string command = workingDirectory + @"Teleopti.Support.Security.exe";
            ProcessStartInfo processStartInfo = new ProcessStartInfo(command);
            stringBuilder.Append(SPACE);
            stringBuilder.Append(@"-DS");
            stringBuilder.Append(nHibDataSource.ServerName + SPACE);
            stringBuilder.Append(getLogonString(sqlConnectionStringBuilder));
            stringBuilder.Append(@"-DD");
            stringBuilder.Append(nHibDataSource.DatabaseName + SPACE);
            stringBuilder.Append(@"-CD");
            stringBuilder.Append(nHibDataSource.DatabaseName + SPACE);
            processStartInfo.Arguments = stringBuilder.ToString();

            processStartInfo.WorkingDirectory = workingDirectory;

            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = true;
            return processStartInfo;
        }


        //          2- If TeleoptiCCC7 DbType
        //          Teleopti.Support.Security.exe -DS%MyServerInstance% -DD%DATABASE% -EE
        private ProcessStartInfo createProcessStartInfoForApplicationSecurity(NHibDataSource nHibDataSource)
        {
            StringBuilder stringBuilder = new StringBuilder();
            SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(nHibDataSource.ConnectionString);
            string workingDirectory = _teleoptiCccBaseInstallFolder + @"DatabaseInstaller\Enrypted\";

            string command = workingDirectory + @"Teleopti.Support.Security.exe";
            ProcessStartInfo processStartInfo = new ProcessStartInfo(command);
            stringBuilder.Append(SPACE);
            stringBuilder.Append(@"-DS");
            stringBuilder.Append(nHibDataSource.ServerName + SPACE);
            stringBuilder.Append(getLogonString(sqlConnectionStringBuilder));
            stringBuilder.Append(@"-DD");
            stringBuilder.Append(nHibDataSource.DatabaseName + SPACE);
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
            SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(nHibDataSource.ConnectionString);
            string workingDirectory = _teleoptiCccBaseInstallFolder + @"DatabaseInstaller\";

            string command = workingDirectory + @"DBManager.exe";
            ProcessStartInfo processStartInfo = new ProcessStartInfo(command);
            stringBuilder.Append(SPACE);
            stringBuilder.Append(@"-S");
            stringBuilder.Append(sqlConnectionStringBuilder.DataSource + SPACE);
            stringBuilder.Append(getLogonStringForDBManager(sqlConnectionStringBuilder));
            stringBuilder.Append(@"-D");
            stringBuilder.Append(nHibDataSource.DatabaseName + SPACE);
            stringBuilder.Append(@"-O");
            stringBuilder.Append(getCccDbType(databaseTypeString) + SPACE);
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
                case Nhib.ApplicationDatabaseTextConstant :
                    return "TeleoptiCCC7";
                case Nhib.AnalyticsDatabaseTextConstant :
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
            //buttonBack.Enabled = false;
            foreach (ListViewItem listViewItem in listViewDatabases.SelectedItems)
            {
                if (listViewItem.ImageIndex == ImageIndexDatabaseVersionNotOk)
                {
                    buttonUpdate.Enabled = true;
                    break;
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
            NHibDataSource nHibDataSource = (NHibDataSource) listViewItem.Tag;
            listViewItem.ImageIndex = newImageIndex;
            switch (newImageIndex)
            {
                case ImageIndexDatabaseNotConnected:
                    listViewItem.SubItems[2].Text = NHibDataSource.NotConnected;
                    break;
                case ImageIndexDatabaseVersionNotOk :
                case ImageIndexDatabaseVersionOk :
                    listViewItem.SubItems[2].Text = nHibDataSource.Version;
                    if (nHibDataSource.CccDatabaseType == Nhib.AnalyticsDatabaseTextConstant)
                    {
                        DBHelper dbHelper = new DBHelper(nHibDataSource.ConnectionString);
                        string aggDbName = dbHelper.GetAggDatabaseName();
                        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(nHibDataSource.ConnectionString);
                        builder.InitialCatalog = aggDbName;
                        NHibDataSource aggNHibDataSource = new NHibDataSource(nHibDataSource.FactoryName, builder.ConnectionString, Nhib.AggregationDatabaseTextConstant);
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

        


        private void SetListviewIcon(NHibDataSource nHibDataSource, ListViewItem listViewItem)
        {
            
            DBHelper dbHelper = new DBHelper(nHibDataSource.ConnectionString);
            if (dbHelper.TestConnection())
            {
                nHibDataSource.Version = dbHelper.GetDatabaseVersion();
                Version dbVersion = new Version(nHibDataSource.Version);
                if (
                    dbVersion.Major < _currentVersion.Major ||
                    dbVersion.Minor < _currentVersion.Minor ||
                    dbVersion.Build < _currentVersion.Build
                    )
                {
                    CallSetListViewIcon(ImageIndexDatabaseVersionNotOk, listViewItem.Index);
                }
                else
                {
                    CallSetListViewIcon(ImageIndexDatabaseVersionOk, listViewItem.Index);
                }
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
                    Nhib nhib = (Nhib) preselectedListViewItem.Group.Tag;
                    NotepadStarter notepadStarter = new NotepadStarter(nhib.FileName);
                    _notepadStarters.Add(notepadStarter);
                    notepadStarter.NotepadExited += new NotepadStarter.NotepadExitedDelegate(notepadStarter_NotepadExited);
                    notepadStarter.Run();
                }
            }

        }

        void notepadStarter_NotepadExited(object sender, EventArgs e)
        {
            NotepadStarter notepadStarter = (NotepadStarter) sender;
            _notepadStarters.Remove(notepadStarter);
        }
    }
}
