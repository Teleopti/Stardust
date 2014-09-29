using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Teleopti.Support.Tool.Controls.DatabaseDeployment.Pages
{
    public partial class Configure7ZipDllLocation : SelectionPage
    {
        private readonly DatabaseDeploymentModel _model;
        private readonly string NameOf7zDll = "7z.dll";

        public Configure7ZipDllLocation()
        {
            InitializeComponent();
        }

        public Configure7ZipDllLocation(DatabaseDeploymentModel model) : this()
        {
            _model = model;
            textBoxSevenZipDllUrn.Text = _model.SettingsInRegistry.LocationOf7zDll;
        }


        public override void GetData()
        {
            //_model.SevenZipDllUrn
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog(this);
            textBoxSevenZipDllUrn.Text = openFileDialog1.FileName;
        }

        private void smoothLink1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //http://www.7-zip.org/
            ProcessStartInfo sInfo = new ProcessStartInfo("http://www.7-zip.org/");
            Process.Start(sInfo);


        }

        private void textBoxSevenZipDllUrn_TextChanged(object sender, EventArgs e)
        {
            if (IsValid7zDllFile(textBoxSevenZipDllUrn.Text))
            {
                _model.SettingsInRegistry.LocationOf7zDll = textBoxSevenZipDllUrn.Text;
                _model.SettingsInRegistry.SaveAll();
                triggerHasValidInput(true);
            }
            else
            {
                _model.SettingsInRegistry.LocationOf7zDll = string.Empty;
                triggerHasValidInput(false);
            }
        }

        public override bool ContentIsValid()
        {
            return IsValid7zDllFile(textBoxSevenZipDllUrn.Text);
        }

        private bool IsValid7zDllFile(string dllUrn)
        {
            if (!File.Exists(dllUrn)) { return false; }
            FileInfo dllFile = new FileInfo(dllUrn);
            if (dllFile.Name != NameOf7zDll) { return false; }
            return true;
        }

    }
}
