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
    public partial class MainChangeDBSettings : UserControl
    {
        private DBHelper _db;
        private readonly LinkedList<UserControl> _li;
        private List<ProgressItem> _progressList = new List<ProgressItem>();
        private int _controlPos = -1;
        private readonly MainForm _mainForm;
        private readonly ConnectionSettings _connStringSetting;
        private string _confFilesRoot; // The Root that configFile expects us to stand in.
        private Mode _mode = Mode.Installed;
        private string _logFile;
        private string _fileKey;
         
      
        /// <summary>
        /// Defines what Mode/Environment we are running the app in
        /// </summary>
        private enum Mode
        {
            Installed,
            Debug,
            Develop
        }

        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="mainForm">A reference to it's parent form</param>
        public MainChangeDBSettings(MainForm mainForm, DBHelper dbHelper)
        {
            InitializeComponent();
            _mainForm = mainForm;
            configFile = System.Configuration.ConfigurationManager.AppSettings["configFilePath"];
            setConfileParam();
            //TODO remove this 3 lines
            //mode = Mode.installed;
            //confFilesRoot = @"C:\Program Files\Teleopti";
            //fileKey = "ipath";
        
            _li = new LinkedList<UserControl>();
            _connStringSetting = new ConnectionSettings();
            connSummary = new Summary();
            //_li.AddLast(dbConnect);
            _li.AddLast(_connStringSetting);
            _li.AddLast(connSummary);

            connSummary.ViewLog.Click += viewLog_Click;

            _connStringSetting.TestConnectButton.Click += testConnectButton_Click;

            _db = dbHelper;
            loadDatabases();
            loadSqlUsers();
            switchControl(1);
            BNext.Enabled = false;
        }


        /// <summary>
        /// Sets Mode/Environment we are running the app in and the Root Path to all the XML files that we need to update
        /// </summary>
        private void setConfileParam()
        {
            string currentPath = System.IO.Directory.GetCurrentDirectory();
            if (Environment.UserDomainName == "TOPTINET" && currentPath.Contains(@"\Teleopti.Support.Tool\"))
            //We are in the domain TOPTINET and the app is running from path \Root\ConfigTool\ = We must be in a develop environment
            {
                _fileKey = "dpath";
                if (currentPath.ToUpperInvariant().Contains(@"\bin\debug".ToUpperInvariant()) || currentPath.ToUpperInvariant().Contains(@"\bin\release".ToUpperInvariant()))
                {
                    //We are in debug or release mode
                    currentPath += @"\..\..";
                    _mode = Mode.Debug;
                }
                else
                {
                    _mode = Mode.Develop;
                }
            }
            else
            {
                _mode = Mode.Installed;
                _fileKey = "ipath";
            }
            _confFilesRoot = System.IO.Directory.GetParent(currentPath).FullName;
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
                        setupSummary();
                        setJobSteps();
                        BNext.Hide();
                        BInstall.Show();

                    }
                    Controls.Add(_li.ElementAt(_controlPos));
                    _li.ElementAt(_controlPos).Location = ConfigFunctions.Center(Width, Height, _li.ElementAt(_controlPos).Width, _li.ElementAt(_controlPos).Height, 0, -50);
                }
                BNext.Enabled = _controlPos+1 < _li.Count();
            }
        }

        /// <summary>
        /// Returns the text for the Mode/Environment we are running in
        /// </summary>
        /// <returns></returns>
        private string getEnvironmentText()
        {
            string mod = "";
            switch (_mode)
            {
                case Mode.Installed:
                    mod = "Installed";
                    break;
                case Mode.Develop:
                    mod = "Develop";
                    break;
                case Mode.Debug:
                    mod = "Develop";
                    break;
            }
            return mod;
        }

        private void copyNhibFiles()
        {
            //// This method will only be runned if in develop or debug mode. 
            //var messagingPath = _confFilesRoot + @"\Teleopti.Messaging\TeleOpti.Messaging.Bin";
            //const string messagingFile = @"\Teleopti.Messaging.Svc.exe.config";
            
            ////if (!System.IO.File.Exists(messagingPath + messagingFile))
            ////{
            //    if (!IOHelper.DirectoryExists(messagingPath))
            //        IOHelper.CreateDirectory(messagingPath);
            //    IOHelper.Copy(System.IO.Directory.GetCurrentDirectory() + messagingFile,messagingPath + messagingFile, true);
            //    IOHelper.WriteFile("Added " +  messagingPath + messagingFile + " as a new file.\r\n", _logFile, true);
            ////}
            string sdkNhibPath = _confFilesRoot + @"\Teleopti.Ccc.Sdk\Teleopti.Ccc.Sdk.Host\bin\debug";
            string clientNhibPath = _confFilesRoot + @"\Teleopti.Ccc.SmartClientPortal\Teleopti.Ccc.SmartClientPortal.Shell\bin\debug";
            const string nhibFile = @"\V7Config.nhib.xml";
            //if (!System.IO.File.Exists(sdkNhibPath + nhibFile))
            //{
            if (!IOHelper.DirectoryExists(sdkNhibPath))
                IOHelper.CreateDirectory(sdkNhibPath);
            IOHelper.Copy(System.IO.Directory.GetCurrentDirectory() + nhibFile, sdkNhibPath + nhibFile, true);
            IOHelper.WriteFile("Added " + sdkNhibPath + nhibFile + " as a new file.\r\n", _logFile, true);
            //IOHelper.WriteFile("File " + sdkNhibPath + nhibFile + " didn't exist. Added this as a new file.\r\n", _logFile, true);
            //               }
            //if (!System.IO.File.Exists(clientNhibPath + nhibFile))
            //{
            if (!IOHelper.DirectoryExists(clientNhibPath))
                IOHelper.CreateDirectory(clientNhibPath);
            IOHelper.Copy(System.IO.Directory.GetCurrentDirectory() + nhibFile, clientNhibPath + nhibFile, true);
            IOHelper.WriteFile("Added " + clientNhibPath + nhibFile + " as a new file.\r\n", _logFile, true);
            //IOHelper.WriteFile("File " + clientNhibPath + nhibFile + " didn't exist. Added this as a new file.\r\n", _logFile, true);
            //}
        }
        

        /// <summary>
        /// Executes All the jobs that is to be executed
        /// </summary>
        private void executeJob()
        {
            foreach (ProgressItem t in _progressList)
                t.Waiting = true;

            IOHelper.CreateDirectory(System.IO.Directory.GetCurrentDirectory() + "\\Logs");
            _logFile = System.IO.Directory.GetCurrentDirectory() + "\\Logs\\ChangeDBConnections" + DateTime.Now.ToString("yyyy-MM-dd HH.mm", System.Globalization.CultureInfo.InvariantCulture) + ".txt";
            IOHelper.WriteFile("Settings\r\n", _logFile, true);
            IOHelper.WriteFile("SQL Server = " + _db.ServerName + "\r\n", _logFile, true);
            IOHelper.WriteFile("Application Database = " + _connStringSetting.ApplicationDB + "\r\n", _logFile, true);
            IOHelper.WriteFile("Aggregation Database = " + _connStringSetting.AggregationDB + "\r\n", _logFile, true);
            IOHelper.WriteFile("Analytic Database = " + _connStringSetting.AnalyticDB + "\r\n", _logFile, true);
            IOHelper.WriteFile("SQL User = " + _connStringSetting.SqlUser + "\r\n", _logFile, true);
            IOHelper.WriteFile("SQL User Password = " + _connStringSetting.SqlUserPassword + "\r\n", _logFile, true);
            IOHelper.WriteFile("Environment = " + getEnvironmentText() + "\r\n", _logFile, true);

            IOHelper.WriteFile("-----------------------------------------------------\r\n\r\n", _logFile, true);
            IOHelper.WriteFile("Ececuting Log\r\n", _logFile, true);

            bool error = false;


            if (_progressList[0].ExecuteStep)
            {
                _progressList[0].Running = true;
                try
                {

                    IOHelper.WriteFile("Start executing step 'Update database references in Database'\r\n", _logFile, true);
                    _db.UpdateCrossRef(_connStringSetting.AggregationDB, _connStringSetting.AnalyticDB);

                    _progressList[0].Finished = true;
                    IOHelper.WriteFile("The step 'Update database references in Database' was executed successfully.\r\n", _logFile, true);

                }
                catch (SqlException ex)
                {
                    _progressList[0].Error = true;
                    error = true;
                    connSummary.ExecuteOk = false;
                    connSummary.ResultText = "The step 'Update database references in Database' failed and a rollback was performed. Execution was stopped. " + ex.Message + " Please see the logfile. \r\n";
                    IOHelper.WriteFile("The step 'Update database references in Database' failed and a rollback was performed. Execution was stopped. " + ex.Message + "\r\n", _logFile, true);
                }
                IOHelper.WriteFile("\r\n", _logFile, true);
            }
            if (_progressList[1].ExecuteStep)
            {
                XmlHandlerMessages xmlMessages;
                if (!error)
                {
                    IOHelper.WriteFile("Start executing step: Update database references in configuration files \r\n", _logFile, true);
                    _progressList[1].Running = true;
                    if ((_mode == Mode.Debug || _mode == Mode.Develop))
                    { //Check if The file Teleopti.Ccc.Sdk\Teleopti.Ccc.Sdk.Host\v7Config.nhib.xml exists otherwise copy from 
                        copyNhibFiles();   
                    }
                    xmlMessages = XmlHandler.UpdateAllConfigurationFiles(configFile, _confFilesRoot, _connStringSetting.ApplicationDB, _connStringSetting.AnalyticDB, _connStringSetting.SqlUser, _connStringSetting.SqlUserPassword, _db.ServerName, _fileKey);
                    _progressList[1].Finished = true;
                    connSummary.ExecuteOk = true;
                    IOHelper.WriteFile("Step 'Update of database references in configuration files' was executed successfully.\r\n", _logFile, true);
                    connSummary.ResultText = "The job was executed successfully. If you want to continue with installing a patch, click here \r\n";
 
                    if (xmlMessages != null && xmlMessages.Errors.Count > 0)
                    {
                        //error = true;
                        _progressList[1].Error = true;
                        connSummary.ExecuteOk = false;
                        IOHelper.WriteFile("\r\n  Step 'Update database references in configuration files'. Configuration files that recieved an error\r\n", _logFile, true);
                        foreach (var err in xmlMessages.Errors)
                        {
                            IOHelper.WriteFile("    " + err + "\r\n", _logFile, true);
                        }

                        IOHelper.WriteFile("\r\n  Step 'Update database references in configuration files'.  Configuration files that recieved an succsess\r\n", _logFile, true);
                        foreach (string message in xmlMessages.Messages)
                        {
                            IOHelper.WriteFile("    " + message + "\r\n", _logFile, true);
                        }

                    }
                }
            }
            connSummary.LogVisible = true;
        }

        /// <summary>
        /// Adds the job steps that is to be executes to the Summary control
        /// </summary>
        private void setJobSteps()
        {
            if (_progressList.Count == 0)
            {
                _progressList = new List<ProgressItem>();
                var p1 = new ProgressItem
                             {
                                 ItemText = "Update database references in Database."
                             };
                var p2 = new ProgressItem
                             {
                                 ItemText = "Update database references in configuration files."
                             };
                _progressList.Add(p1);
                _progressList.Add(p2);
                connSummary.AddJobStep(p1);
                connSummary.AddJobStep(p2);
            }
        }


        /// <summary>
        /// Sets the summary, shows the input parameters that we have chooses
        /// </summary>
        private void setupSummary()
        {
            connSummary.ClearSettings();
            connSummary.AddSetting("SQL Server", _db.ServerName);
            connSummary.AddSetting("Application Database", _connStringSetting.ApplicationDB);
            connSummary.AddSetting("Aggregation Database", _connStringSetting.AggregationDB);
            connSummary.AddSetting("Analytic Database", _connStringSetting.AnalyticDB);
            connSummary.AddSetting("SQL User Acount", _connStringSetting.SqlUser);
            connSummary.AddSetting("SQL User Password", _connStringSetting.SqlUserPassword);
            connSummary.AddSetting("Environment", getEnvironmentText());   
        }

        /// <summary>
        /// Loads the all the databses that we can choose from
        /// </summary>
        private void loadDatabases()
        {
            if (_db != null && _db.TestConnection(null))
            {
                IEnumerable<string> databases = _db.Databases;
                _connStringSetting.FillDatabases(databases.ToList(), databases.ToList(), databases.ToList());
            }
            else
                _connStringSetting.FillDatabases(null, null, null);
        }

        /// <summary>
        /// Loads all the SQL users that we can choose from
        /// </summary>
        private void loadSqlUsers()
        {
            if (_db != null && _db.TestConnection(null))
                _connStringSetting.FillSqlUsers(_db.DBUsers);
            else
                _connStringSetting.FillSqlUsers(null);
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

        /// <summary>
        /// Handles Execute click, will trigger the job to execute
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BInstall_Click(object sender, EventArgs e)
        {
            executeJob();
        }

        #region ConnStringSettings functionality

        /// <summary>
        /// handels TestConnection click
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="ea">The Events</param>
        private void testConnectButton_Click(object sender, EventArgs ea)
        {
            BNext.Enabled = false;
            Cursor = Cursors.WaitCursor;
            _connStringSetting.ConnectedColor = Color.Black;
            _connStringSetting.TestConnection = "Connecting...";
            _connStringSetting.RefreshConnected();
            try
            {
                DBHelper tempDb = new DBHelper(_db.ServerName, _connStringSetting.SqlUser,
                                               _connStringSetting.SqlUserPassword);
                List<string> databases = new List<string>();
                databases.Add(_connStringSetting.AggregationDB);
                databases.Add(_connStringSetting.ApplicationDB);
                databases.Add(_connStringSetting.AnalyticDB);
                connStringConnected(tempDb.ConnectType(databases));
                
                if (HaveDatabasesBeenSelected() && tempDb.ConnectType(databases) != DataLayer.Right.None)
                    BNext.Enabled = true;
            }
            catch (SqlException ex)
            {
                _connStringSetting.ConnectedColor = Color.Red;
                _connStringSetting.TestConnection = "Not Connected: " + ex.Message;
            }
            Cursor = Cursors.Default;
        }

        private bool HaveDatabasesBeenSelected()
        {
            return !string.IsNullOrEmpty(_connStringSetting.ApplicationDB) && _connStringSetting.ApplicationDB != "---------------" &&
                   !string.IsNullOrEmpty(_connStringSetting.AnalyticDB) && _connStringSetting.AnalyticDB != "---------------" &&
                   !string.IsNullOrEmpty(_connStringSetting.AggregationDB) && _connStringSetting.AggregationDB != "---------------";
        }

        /// <summary>
        /// Sets the Connection text
        /// </summary>
        /// <param name="dbRights"></param>
        private void connStringConnected(Right dbRights)
        {
            switch (dbRights)
            {
                case DataLayer.Right.SA:
                    _connStringSetting.ConnectedColor = Color.Green;
                    _connStringSetting.TestConnection = "Connected as System Admin";
                    break;
                case DataLayer.Right.DBOwner:
                    _connStringSetting.ConnectedColor = Color.Green;
                    _connStringSetting.TestConnection = "Connected as Database Owner";
                    break;
                case DataLayer.Right.Limited:
                    _connStringSetting.ConnectedColor = Color.Orange;
                    _connStringSetting.TestConnection = "Connected with limited access";
                    break;
                case DataLayer.Right.None:
                    _connStringSetting.ConnectedColor = Color.Red;
                    _connStringSetting.TestConnection = "Not Connected";
                    break;
            }
        }
        #endregion

        /// <summary>
        /// Handels viewLog click
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="ea">The Events</param>
        private void viewLog_Click(object sender, EventArgs ea)
        {
            processHelper.Start(_logFile);
        }

    }
}
