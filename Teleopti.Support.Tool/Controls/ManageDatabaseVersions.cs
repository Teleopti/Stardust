using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
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
        private readonly MainForm _mainForm;
        private readonly Version _currentVersion;

        public ManageDatabaseVersions(MainForm mainForm, Version currentVersion)
        {
            _mainForm = mainForm;
            _currentVersion = currentVersion;
            InitializeComponent();
        }

        private void ManageDatabaseVersions_Load(object sender, EventArgs e)
        {
            smoothLabelCurrentVersion.Text = _currentVersion.ToString();
            string installFolder = (string) Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Teleopti\TeleoptiCCC\InstallationSettings", "INSTALLDIR", @"C:\");
            textBoxNHibFolder.Text = installFolder + @"TeleoptiCCC\SDK\";
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
            buttonUpdate.Enabled = false;
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
            SetButtonStates(false);
            listViewDatabases.Visible = false;
            textBoxOutput.Visible = true;
            execute(@"C:\Temp\");
        }

        private void SetButtonStates(bool state)
        {
            buttonBack.Enabled = state;
            buttonRefresh.Enabled = state;
            buttonUpdate.Enabled = state;
        }

        delegate void EnableBackDelegate();

        private void EnableBack()
        {
            buttonBack.Enabled = true;
        }

        private void execute(string workingDirectory)
        {
            string command = @"cmd";
            // create the ProcessStartInfo using "cmd" as the program to be run, and "/c " as the parameters.
            // Incidentally, /c tells cmd that we want it to execute the command that follows, and then exit.
            ProcessStartInfo procStartInfo = new ProcessStartInfo(command, @" /c " + workingDirectory + @"DirWin.bat");

            procStartInfo.WorkingDirectory = workingDirectory;
            //This means that it will be redirected to the Process.StandardOutput StreamReader.
            procStartInfo.RedirectStandardOutput = true;
            //This means that it will be redirected to the Process.StandardError StreamReader. (same as StdOutput)
            procStartInfo.RedirectStandardError = true;

            procStartInfo.UseShellExecute = false;
            // Do not create the black window.
            procStartInfo.CreateNoWindow = true;
            // Now we create a process, assign its ProcessStartInfo and start it
            Process proc = new Process();

            //This is importend, else some Events will not fire!
            proc.EnableRaisingEvents = true;

            // The given Funktion will be raised if the Process wants to print an output to consol                    
            proc.OutputDataReceived += new DataReceivedEventHandler(ProcOnOutputDataReceived);
            // Std Error
            proc.ErrorDataReceived += new DataReceivedEventHandler(ProcOnErrorDataReceived);
            // If Batch File is finished this Event will be raised
            proc.Exited += new EventHandler(ProcOnExited);
            // passing the Startinfo to the process
            proc.StartInfo = procStartInfo;
            proc.Start();
            proc.BeginOutputReadLine();
            //textBoxOutput.Text = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();
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
            textBoxOutput.ScrollToCaret();
            //textBoxOutput.AppendText(@"\n");
        }

        private void ProcOnExited(object sender, EventArgs eventArgs)
        {
            buttonBack.Invoke(new EnableBackDelegate(EnableBack));
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
            buttonUpdate.Enabled = !e.Item.SubItems[2].Text.Equals(_currentVersion.ToString());
        }
    }
}
