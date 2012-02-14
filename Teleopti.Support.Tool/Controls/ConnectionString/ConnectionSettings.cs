using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Teleopti.Support.Tool.Controls.General;

namespace Teleopti.Support.Tool.Controls.ConnectionString
{
    public partial class ConnectionSettings : UserControl
    {
        public ConnectionSettings()
        {
            InitializeComponent();
        }

          public void FillDatabases(IEnumerable<string> applicationDatabases, IEnumerable<string> aggregationDatabases, IEnumerable<string> analyticDatabases)
      
        {
            dbSelect.FillDatabases(applicationDatabases,aggregationDatabases,analyticDatabases);
        }

        public void FillSqlUsers(IEnumerable<string> users)
        {
            sqlAccount.FillSqlUsers(users);
           
           
        }


         public string AggregationDB
        {
           get { return dbSelect.AggregationDB; }

        }



        public string ApplicationDB
        {
            get {return dbSelect.ApplicationDB;} 

        }

        public string AnalyticDB
        {
            get { return dbSelect.AnalyticDB; } 

        }

        public string SqlUser
        {
            get { return sqlAccount.SqlUser; }
           
        }

        public string SqlUserPassword
        {
            get { return sqlAccount.SqlUserPassword;}
            set { sqlAccount.SqlUserPassword = value; }
           
        }
         public string TestConnection
        {
            get { return sqlAccount.TestConnection; }
            set { sqlAccount.TestConnection = value; }
           
        }
         public string GroupText
         {
             get { return groupBox1.Text; }
             set { groupBox1.Text = value; }
         }

         public Color ConnectedColor
         {
             get { return sqlAccount.ConnectedColor; }
             set { sqlAccount.ConnectedColor = value; }

         }

        public Button TestConnectButton
        {
            get { return sqlAccount.TestConnectButton; }
        }

        public void RefreshConnected()
        {
            sqlAccount.RefreshConnected();
        }

       
    }
}
