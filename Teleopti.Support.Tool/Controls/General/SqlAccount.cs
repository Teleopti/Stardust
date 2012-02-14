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
    public partial class SqlAccount : UserControl
    {
        public SqlAccount()
        {
            InitializeComponent();
        }


        public void FillSqlUsers(IEnumerable<string> users)
        {
            CNewSqlUser.DataSource = users;
           
        }

       




        public string SqlUser
        {
            get { return (string)CNewSqlUser.SelectedItem; }
         }

        public string SqlUserPassword
        {
            get { return CNewSqlUserPwd.Text; }
            set { CNewSqlUserPwd.Text = value; }

        }
        public string TestConnection
        {
            set { LTestConnection.Text = value; }
            get { return LTestConnection.Text; }

        }
      

        public Color ConnectedColor
        {
            get { return LTestConnection.ForeColor; }
            set { LTestConnection.ForeColor = value; }

        }

        public Button TestConnectButton
        {
            get { return BTestConnection; }
        }

        public void RefreshConnected()
        {
            LTestConnection.Refresh();
        }
    }
}
