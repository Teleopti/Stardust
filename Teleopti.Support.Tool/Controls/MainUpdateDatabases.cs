using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Data.SqlClient;
using Teleopti.Support.Tool.DataLayer;
using Teleopti.Support.Tool.Controls.General;
using Teleopti.Support.Tool.Controls.ConnectionString;

namespace Teleopti.Support.Tool.Controls
{
    public partial class MainUpadateDabases : UserControl
    {
        private DBHelper _db;
        private readonly LinkedList<UserControl> _li;
        //private List<ProgressItem> _progressList = new List<ProgressItem>();
        private int _controlPos = -1;
        private readonly MainForm _mainForm;
        //private readonly ConnectionSettings _connStringSetting;
        //private string _confFilesRoot; // The Root that configFile expects us to stand in.
        private string _logFile;
        // private string _fileKey;



        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="mainForm">A reference to it's parent form</param>
        public MainUpadateDabases(MainForm mainForm)
        {
            InitializeComponent();
            this._mainForm = mainForm;
            // XmlHandler.GetNihibSettings(System.IO.Directory.GetCurrentDirectory() + @"\..\TeleoptiCCC\SDK\TeleoptiCCC7.nhib.xml");

            IList<Nhib> Nhibs = XmlHandler.GetNihibSettings(@"C:\Program Files (x86)\Teleopti\TeleoptiCCC\SDK\").ToList();

            // configFile = System.Configuration.ConfigurationManager.AppSettings["configFilePath"];

            //_li = new LinkedList<UserControl>();
            //dbConnect = new DBConnect();
            //_connStringSetting = new ConnectionSettings();
            //connSummary = new Summary();
            //_li.AddLast(dbConnect);
            //_li.AddLast(_connStringSetting);
            //_li.AddLast(connSummary);
               
           // switchControl(1);
            BNext.Enabled = false;
        }

        
        /// <summary>
        /// Handeles Next and Back button click
        /// </summary>
        /// <param name="step"></param>
        private void switchControl(int step)
        {
            _controlPos += step;
            if (_controlPos < 0)
            {
                _mainForm.ShowPTracks();
                Hide();
            }
            else
            {
                if (_controlPos < _li.Count())
                {
                    if (_controlPos - step >= 0)
                    {
                        Controls.Remove(_li.ElementAt(_controlPos - step));
                        if (_li.ElementAt(_controlPos - step).Equals(connSummary))
                        {
                            BNext.Show();
                            BInstall.Hide();
                        }
                    }
                    if (_li.ElementAt(_controlPos).Equals(connSummary))
                    {
                        //Uppdate the connSummary control with all parameters that it needs

                        BNext.Hide();
                        BInstall.Show();

                    }
                    Controls.Add(_li.ElementAt(_controlPos));
                    _li.ElementAt(_controlPos).Location = ConfigFunctions.Center(Width, Height, _li.ElementAt(_controlPos).Width, _li.ElementAt(_controlPos).Height, 0, -50);
                }
                BNext.Enabled = _controlPos + 1 < _li.Count();
            }
        }


        /// <summary>
        /// handles Back button click
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The Events</param>
        private void BBack_Click(object sender, EventArgs e)
        {
            switchControl(-1);
        }

        /// <summary>
        /// Handles Next button click
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The Events</param>
        private void BNext_Click(object sender, EventArgs e)
        {
            switchControl(1);
        }


    }
}
