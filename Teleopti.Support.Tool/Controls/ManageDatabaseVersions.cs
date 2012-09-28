using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Teleopti.Support.Tool.Controls
{
    public partial class ManageDatabaseVersions : UserControl
    {
        private readonly MainForm _mainForm;

        public ManageDatabaseVersions(MainForm mainForm)
        {
            _mainForm = mainForm;
            InitializeComponent();
        }

        private void ManageDatabaseVersions_Load(object sender, EventArgs e)
        {
            textBoxNHibFolder.Text = @"C:\";
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            _mainForm.ShowPTracks();
            Hide();
        }
    }
}
