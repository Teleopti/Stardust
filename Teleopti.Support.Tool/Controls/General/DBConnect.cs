using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Teleopti.Support.Tool.Controls.General
{
    public partial class DBConnect : UserControl
    {
       

          public DBConnect()
        {
            InitializeComponent();
          
        }
        
        
        private void CBWindowsAuth_CheckedChanged(object sender, EventArgs e)
        {
            if (CBWindowsAuth.Checked)
            {
                TBSQLUser.Enabled = false;
                TBPassword.Enabled = false;
            }
            else
            {
                TBSQLUser.Enabled = true;
                TBPassword.Enabled = true;
            }
        }

    
        public string Server
        {
            get { return TBServer.Text; }
            set { TBServer.Text = value; }
        }
        public string User
        {
            get {return TBSQLUser.Text;}
            set { TBSQLUser.Text = value; }
           
        }

        public string Password
        {
            get { return TBPassword.Text; }
            set { TBPassword.Text = value; }
            
        }

        public string Connected
        {
            get { return LConnected.Text; }
            set { LConnected.Text = value; }
        }
        public bool WindowsAuth
        {
            get { return CBWindowsAuth.Checked; }
            set { CBWindowsAuth.Checked = value; }
        }

        public Color ConnectedColor
        {
            set { LConnected.ForeColor = value; }
            get { return LConnected.ForeColor; }
        }

        public Button ConnectButton
        {
            get { return BConnect; }
        }

        public void RefreshConnected()
        {
            LConnected.Refresh();
        }

        private void BConnect_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(TBServer.Text))
                TBServer.Text = ".";
        }

       
    }
}
