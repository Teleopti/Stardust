using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Teleopti.Support.Tool.DataLayer;
using Teleopti.Support.Tool.Controls.General;
using System.Data.SqlClient;

namespace Teleopti.Support.Tool.Controls
{
    public partial class MainPatch : UserControl
    {
        private MainForm mainForm;
    
        private DBConnect dbConnect = null;
        private DBSelect dbSelect = null;
        private DBHelper db;
        private LinkedList<UserControl> li = null;
        private int controlPos = -1;
        public MainPatch(MainForm mainForm)
        {
            InitializeComponent();
             this.mainForm = mainForm;
          
            li = new LinkedList<UserControl>();
            dbConnect = new DBConnect();
            dbSelect = new DBSelect();
            li.AddLast(dbConnect);
            li.AddLast(dbSelect);
            //Load the first Control that should be visible

            switchControl(1);
            this.BNext.Enabled = false;
            dbConnect.ConnectButton.Click += new EventHandler(connectButton_Click);

        }

    
        private void switchControl(int step)
        {
            controlPos += step;
            if (controlPos < 0)
            {
                mainForm.ShowPTracks();
                this.Hide();
            }
            else
            {
                if (controlPos < li.Count())
                {
                    if (controlPos - step >= 0)
                    {
                        this.Controls.Remove(li.ElementAt(controlPos - step));
                       
                    }
                   
                    this.Controls.Add(li.ElementAt(controlPos));
                    li.ElementAt(controlPos).Location = ConfigFunctions.Center(this.Width, this.Height, li.ElementAt(controlPos).Width, li.ElementAt(controlPos).Height, 0, -50);
                }
                if (controlPos+1>= li.Count())
                {
                    this.BNext.Enabled = false;
                }
                else
                {
                    this.BNext.Enabled = true;
                }
            }
        }

        private void loadDatabases()
        {
            if (db != null && db.TestConnection(null))
            {
                IEnumerable<string> databases = db.Databases;
                dbSelect.FillDatabases(databases.ToList<string>(), databases.ToList<string>(), databases.ToList<string>());
              
            }
            else
            {
                dbSelect.FillDatabases(null,null,null);
               
            }
        }

        private void BBack_Click(object sender, EventArgs e)
        {
            switchControl(-1);
        }

        private void BNext_Click(object sender, EventArgs e)
        {
            switchControl(1);
        }

      

        private void connectButton_Click(object sender, EventArgs ea)
        {

            //Connect to the database
            Cursor = Cursors.WaitCursor;
            dbConnect.ConnectedColor = Color.Black;
            dbConnect.Connected = "Connecting...";
            dbConnect.RefreshConnected();
            if (dbConnect.WindowsAuth)
                db = new DBHelper(dbConnect.Server);
            else
                db = new DBHelper(dbConnect.Server, dbConnect.User, dbConnect.Password);
            List<string> databases = new List<string>();
            databases.Add("master");
            bool connected = false;
            try
            {
                switch (db.ConnectType(databases))
                {
                    case DataLayer.Right.SA:
                        dbConnect.ConnectedColor = Color.Green;
                        dbConnect.Connected = "Connected as System Admin";
                        connected = true;
                        break;
                    case DataLayer.Right.Limited:
                        dbConnect.ConnectedColor = Color.Orange;
                        dbConnect.Connected = "Connected with limited access";
                        connected = true;
                        break;
                    default:
                        dbConnect.ConnectedColor = Color.Red;
                        dbConnect.Connected = "Not Connected: ";
                        break;
                }
                if (connected)
                {
                    BNext.Enabled = true;
                    loadDatabases();
                  
                  
                }
            }
            catch (SqlException ex)
            {
                dbConnect.ConnectedColor = Color.Red;
                dbConnect.Connected = "Not Connected: " + ex.Message;
            }
            Cursor = Cursors.Default;
        }
    }
}
