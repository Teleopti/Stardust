using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
            _mainForm.ShowPTracks();
            Hide();
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
                ListViewItem[] listViewItems = CreateDatabaseListViewItems(nhib);
                listViewDatabases.Items.AddRange(listViewItems);
            }
            listViewDatabases.EndUpdate();
            _mainForm.Cursor = Cursors.Default;
        }

        private ListViewItem[] CreateDatabaseListViewItems(Nhib nhib)
        {
            Collection<ListViewItem> items = new Collection<ListViewItem>();
            ListViewItem listViewItem;
            if (!listViewDatabases.Items.ContainsKey(nhib.CCCDatabase))
            {
                listViewItem = CreateDatabaseListViewItem(nhib.CCCDatabase, ApplicationDatabaseTextConstant, nhib.CCCVersion);
                listViewItem.Tag = nhib;
                items.Add(listViewItem);
            }
            if (!listViewDatabases.Items.ContainsKey(nhib.AnalyticDatabase))
            {
                listViewItem = CreateDatabaseListViewItem(nhib.AnalyticDatabase, AnalyticsDatabaseTextConstant, nhib.AnalyticVersion);
                listViewItem.Tag = nhib;
                items.Add(listViewItem);
            }
            if (!listViewDatabases.Items.ContainsKey(nhib.AggregationDatabase))
            {
                listViewItem = CreateDatabaseListViewItem(nhib.AggregationDatabase, AggregationDatabaseTextConstant, nhib.AggVersion);
                listViewItem.Tag = nhib;
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
            ListViewItem.ListViewSubItem listViewSubItem = new ListViewItem.ListViewSubItem(listViewItem, type);
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
            // TODO: Köra bat-fil eller något annat
        }

        private void listViewDatabases_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            buttonUpdate.Enabled = !e.Item.SubItems[2].Text.Equals(_currentVersion.ToString());
        }
    }
}
