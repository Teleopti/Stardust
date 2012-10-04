using System;
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
        private const string ApplicationDatabaseTextConstant = "Application DB";
        private const string AnalyticsDatabaseTextConstant = "Analytics DB";
        private const string AggregationDatabaseTextConstant = "Aggregation DB";
        private const string DOUBLEQUOTE = @"""";
        private const String SPACE = " ";
        private readonly MainForm _mainForm;
        private readonly Version _currentVersion;
        private string _teleoptiCccBaseInstallFolder;


        public ManageDatabaseVersions(MainForm mainForm, Version currentVersion)
        {
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
            //buttonUpdate.Enabled = false;
            _mainForm.Cursor = Cursors.WaitCursor;
            listViewDatabases.BeginUpdate();
            listViewDatabases.Items.Clear();
            IList<Nhib> nhibs = XmlHandler.GetNihibSettings(textBoxNHibFolder.Text).ToList();
            foreach (Nhib nhib in nhibs)
            {
                ListViewGroup listViewGroup = listViewDatabases.Groups.Add(nhib.Factoryname, nhib.Factoryname);
                ListViewItem[] listViewItems = CreateDatabaseListViewItems(nhib, listViewGroup);
                listViewDatabases.Items.AddRange(listViewItems);
            }
            listViewDatabases.EndUpdate();
            _mainForm.Cursor = Cursors.Default;
        }

        private ListViewItem[] CreateDatabaseListViewItems(Nhib nhib, ListViewGroup listViewGroup)
        {
            Collection<ListViewItem> items = new Collection<ListViewItem>();
            ListViewItem listViewItem;
            if (!listViewDatabases.Items.ContainsKey(nhib.CCCDatabase))
            {
                listViewItem = CreateDatabaseListViewItem(nhib.CCCDatabase, ApplicationDatabaseTextConstant, nhib.CCCVersion);
                listViewItem.Tag = nhib;
                listViewItem.Group = listViewGroup;
                items.Add(listViewItem);
            }
            if (!listViewDatabases.Items.ContainsKey(nhib.AnalyticDatabase))
            {
                listViewItem = CreateDatabaseListViewItem(nhib.AnalyticDatabase, AnalyticsDatabaseTextConstant, nhib.AnalyticVersion);
                listViewItem.Tag = nhib;
                listViewItem.Group = listViewGroup;
                items.Add(listViewItem);
            }
            if (!listViewDatabases.Items.ContainsKey(nhib.AggregationDatabase))
            {
                listViewItem = CreateDatabaseListViewItem(nhib.AggregationDatabase, AggregationDatabaseTextConstant, nhib.AggVersion);
                listViewItem.Tag = nhib;
                listViewItem.Group = listViewGroup;
                items.Add(listViewItem);
            }
            return items.ToArray();
        }

        private ListViewItem CreateDatabaseListViewItem(string databaseName, string type, string version)
        {
            ListViewItem listViewItem = new ListViewItem(databaseName);
            listViewItem.Name = databaseName;
            if (version.Equals(_currentVersion.ToString()))
            {
                listViewItem.ImageIndex = 0;
            }
            else
            {
                listViewItem.ImageIndex = 1;
            }
            ListViewItem.ListViewSubItem listViewSubItem;
            listViewSubItem = new ListViewItem.ListViewSubItem(listViewItem, type);
            listViewItem.SubItems.Add(listViewSubItem);
            listViewSubItem = new ListViewItem.ListViewSubItem(listViewItem, version);
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
                    Nhib nhib = (Nhib)listViewItem.Tag;
                    string databaseTypeString = listViewItem.SubItems[1].Text;

                    ProcessStartInfo processStartInfo;
                    processStartInfo = CreateProcessStartInfoForDBManager(nhib, listViewItem, databaseTypeString);
                    RunProcess(processStartInfo);

                    if (databaseTypeString == ApplicationDatabaseTextConstant)
                    {
                        processStartInfo = CreateProcessStartInfoForApplicationSecurity(nhib, listViewItem, databaseTypeString);
                        RunProcess(processStartInfo);
                    }

                    if (databaseTypeString == AnalyticsDatabaseTextConstant)
                    {
                        processStartInfo = CreateProcessStartInfoForAnalyticsSecurity(nhib, listViewItem, databaseTypeString);
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
        private ProcessStartInfo CreateProcessStartInfoForAnalyticsSecurity(Nhib nhib, ListViewItem listViewItem, string databaseTypeString)
        {
            StringBuilder stringBuilder = new StringBuilder();
            SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(nhib.CCCConnectionString);
            string workingDirectory = _teleoptiCccBaseInstallFolder + @"DatabaseInstaller\Enrypted\";

            string command = workingDirectory + @"Teleopti.Support.Security.exe";
            ProcessStartInfo processStartInfo = new ProcessStartInfo(command);
            stringBuilder.Append(SPACE);
            stringBuilder.Append(@"-DS");
            stringBuilder.Append(sqlConnectionStringBuilder.DataSource + SPACE);
            stringBuilder.Append(GetLogonString(sqlConnectionStringBuilder));
            stringBuilder.Append(@"-DD");
            stringBuilder.Append(listViewItem.Name + SPACE);
            stringBuilder.Append(@"-CD");
            stringBuilder.Append(nhib.AggregationDatabase + SPACE);
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
        private ProcessStartInfo CreateProcessStartInfoForApplicationSecurity(Nhib nhib, ListViewItem listViewItem, string databaseTypeString)
        {
            StringBuilder stringBuilder = new StringBuilder();
            SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(nhib.CCCConnectionString);
            string workingDirectory = _teleoptiCccBaseInstallFolder + @"DatabaseInstaller\Enrypted\";

            string command = workingDirectory + @"Teleopti.Support.Security.exe";
            ProcessStartInfo processStartInfo = new ProcessStartInfo(command);
            stringBuilder.Append(SPACE);
            stringBuilder.Append(@"-DS");
            stringBuilder.Append(sqlConnectionStringBuilder.DataSource + SPACE);
            stringBuilder.Append(GetLogonString(sqlConnectionStringBuilder));
            stringBuilder.Append(@"-DD");
            stringBuilder.Append(listViewItem.Name + SPACE);
            processStartInfo.Arguments = stringBuilder.ToString();

            processStartInfo.WorkingDirectory = workingDirectory;

            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = true;
            return processStartInfo;
        }

        private string GetLogonString(SqlConnectionStringBuilder sqlConnectionStringBuilder)
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
        private ProcessStartInfo CreateProcessStartInfoForDBManager(Nhib nhib, ListViewItem listViewItem, string databaseTypeString)
        {
            StringBuilder stringBuilder = new StringBuilder();
            SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(nhib.CCCConnectionString);
            string workingDirectory = _teleoptiCccBaseInstallFolder + @"DatabaseInstaller\";

            string command = workingDirectory + @"DBManager.exe";
            ProcessStartInfo processStartInfo = new ProcessStartInfo(command);
            stringBuilder.Append(SPACE);
            stringBuilder.Append(@"-S");
            stringBuilder.Append(sqlConnectionStringBuilder.DataSource + SPACE);
            stringBuilder.Append(GetLogonStringForDBManager(sqlConnectionStringBuilder));
            stringBuilder.Append(@"-D");
            stringBuilder.Append(listViewItem.Name + SPACE);
            stringBuilder.Append(@"-O");
            stringBuilder.Append(GetCccDbType(databaseTypeString) + SPACE);
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

        private string GetLogonStringForDBManager(SqlConnectionStringBuilder sqlConnectionStringBuilder)
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

        private string GetCccDbType(string databaseTypeString)
        {

            switch (databaseTypeString)
            {
                case ApplicationDatabaseTextConstant :
                    return "TeleoptiCCC7";
                case AnalyticsDatabaseTextConstant :
                    return "TeleoptiAnalytics";
                case AggregationDatabaseTextConstant:
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
            //buttonUpdate.Enabled = !e.Item.SubItems[2].Text.Equals(_currentVersion.ToString());
        }

        private void listViewDatabases_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
