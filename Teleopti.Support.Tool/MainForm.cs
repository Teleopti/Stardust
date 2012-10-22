using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Teleopti.Support.Tool.Controls;


namespace Teleopti.Support.Tool
{
    public partial class MainForm : Form
    {
        private UserControl activeControl;
        private Version _productVersion;

        public MainForm()
        {
            InitializeComponent();
        }

        

     

        public void ShowPTracks()
        {
          this.PTracks.Show();
        }

        public void HidePTracks()
        {
            this.PTracks.Hide();
        }

        private void LLChangeDBConn_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //this.Controls.Remove(activeControl);
            panelContent.Controls.Remove(activeControl);
            activeControl = new MainChangeDBSettings(this);
            activeControl.Dock = DockStyle.Fill;
            this.PTracks.Hide();
            panelContent.Controls.Add(activeControl);
            //this.Controls.Add(activeControl);
    
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            panelContent.Controls.Remove(activeControl);
            Version version = new Version(_productVersion.Major, _productVersion.Minor, _productVersion.Build);
            activeControl = new ManageDatabaseVersions(this, version);
            activeControl.Dock = DockStyle.Fill;
            this.PTracks.Hide();
            panelContent.Controls.Add(activeControl);
        }

        
    
      

        private void MainForm_Load(object sender, EventArgs e)
        {
            _productVersion = new Version(Application.ProductVersion);
#if (DEBUG)
            {
                _productVersion = new Version(7, 9, 999);
            }
#endif
            smoothLabelVersion.Text = _productVersion.ToString();
        }

        private void BClose_Click(object sender, EventArgs e)
        {
            Close();
        }


       

     
    }
}
