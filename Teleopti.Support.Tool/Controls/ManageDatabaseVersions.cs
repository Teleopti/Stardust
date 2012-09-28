﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Teleopti.Support.Tool.DataLayer;

namespace Teleopti.Support.Tool.Controls
{
    public partial class ManageDatabaseVersions : UserControl
    {
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
            textBoxNHibFolder.Text = @"C:\"; // TODO: Hämta default SDK folder någonstans
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
            listViewDatabases.BeginUpdate();
            listViewDatabases.Items.Clear();
            IList<Nhib> nhibs = XmlHandler.GetNihibSettings(textBoxNHibFolder.Text).ToList();
            foreach (Nhib nhib in nhibs)
            {
                ListViewItem listViewItem = new ListViewItem(nhib.CCCDatabase);
                listViewItem.ImageIndex = 0;
                ListViewItem.ListViewSubItem listViewSubItem = new ListViewItem.ListViewSubItem(listViewItem, "Application");
                listViewItem.SubItems.Add(listViewSubItem);
                listViewSubItem = new ListViewItem.ListViewSubItem(listViewItem, nhib.CCCVersion);
                listViewItem.SubItems.Add(listViewSubItem);
                listViewDatabases.Items.Add(listViewItem);
            }
            listViewDatabases.EndUpdate();
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            RefreshDatabaseList();
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            // TODO: Köra bat-fil eller något annat
        }
    }
}
