using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
namespace Teleopti.Support.Tool.Controls.General
{
    public partial class DBSelect : UserControl
    {
        public DBSelect()
        {
            InitializeComponent();
        }


        public void FillDatabases(IEnumerable<string> applicationDatabases, IEnumerable<string> aggregationDatabases, IEnumerable<string> analyticDatabases)
        {
            CAggDB.DataSource = aggregationDatabases;
            CAppDB.DataSource = applicationDatabases;
            CAnalyticsDB.DataSource = analyticDatabases;
        }
             
        public string AggregationDB
        {
             get {return (string)CAggDB.SelectedItem;}
        }



        public string ApplicationDB
        {
            get { return (string)CAppDB.SelectedItem; } 
        }

        public string AnalyticDB
        {
         get {return (string)CAnalyticsDB.SelectedItem; }

        }

       
      

     }
}
