using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Teleopti.Support.Tool.Controls.DatabaseDeployment.Pages
{
    public partial class Configure7ZipDllLocation : SelectionPage
    {
        private readonly DatabaseDeploymentModel _model;

        public Configure7ZipDllLocation()
        {
            InitializeComponent();
        }

        public Configure7ZipDllLocation(DatabaseDeploymentModel model) : this()
        {
            _model = model;
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

    }
}
