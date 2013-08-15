using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.Sql;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.DirectoryServices;
using Microsoft.Win32;
using System.Management;
using System.Management.Instrumentation;
using Campari.Software;
using System.IO;
using System.Transactions;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace CheckPreRequisites
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region Main Checks
        private void button1_Click(object sender, EventArgs e)
        {
            //Clear list
            ClearListView();


            //Check web and/or DB server
            if (comboBoxServerSetup.SelectedItem.ToString().Contains("Web"))
            {
                //always run hardware check
                RunHardWareChecks((int)numericUpDownAgents.Value);
                RunWebChecks((int)AgentLevel());
            }
            if (comboBoxServerSetup.SelectedItem.ToString().Contains("DB"))
            {
                //always run hardware check
                RunHardWareChecks((int)numericUpDownAgents.Value);
                RunDBChecks((int)numericUpDownAgents.Value);
            }
            if (comboBoxServerSetup.SelectedItem.ToString().Contains("Version6"))
            {
                RunV6Checks();
            }
        }

        private void CheckDBConnection_Click(object sender, EventArgs e)
        {
            CheckDBInternals();
        }
        #endregion

        #region V6 Checks


        private void RunV6Checks()
        {
            bool DbConn = false;
            string connString = "";

            //Set the connection string
            connString = connStringGet(textBoxDBName.Text);



            //Check that it's possible to connect
            DBConnectionCheck(ref DbConn, connString);



            //if we have a Db-connection, go for the DB-checks
            if (DbConn)
            {
                InsertProcedure(connString);
                DBPreMigrationCheck(connString);
                DBPreMigrationAbsenceTable(connString);
                DBPreMigrationActivitiesTable(connString);
            }

        }

        #endregion


        #region DB Checks
        private void RunDBChecks(int numberOfAgent)
        {
            //Check database components
            CheckDatabaseServices();

            //Check database client softwares
            DBSoftwareCheck();
        }

        #endregion

        #region HardWare Checks
        private void RunHardWareChecks(int numberOfAgent)
        {
            //OS checks correct os + correct architecture 64 bit
            CheckOS();
            CheckComputerName();
            CheckArchitecture();

            //Check ram memory and cpu cores
            CheckMemory(numberOfAgent);
            CheckCPUCores(numberOfAgent);

        }
        #endregion

        #region web checks
        private void RunWebChecks(int numberOfAgent)
        {

            //Check .net framework installed
            CheckNetFx();

            //Check IIS exists and sub components installed
            CheckIIS();

            //Check MVC registered
            //CheckMVC();
        }
        #endregion



        #region DB Checks methods

        private void CheckDBInternals()
        {
            bool DbConn = false;
            string connString = "";

            //Set the connection string
            connString = connStringGet("master");

            //Check that it's possible to connect
            DBConnectionCheck(ref DbConn, connString);

            //if we have a Db-connection, go for the DB-checks
            if (DbConn)
            {
                //Check SQL Server Version
                DBVersionCheck(connString);

                //Check we have sysadmin role
                DBSysAdminCheck(connString);

                //Collation
                DBCollationCheck(connString);

                //WriteTestTempDB
                WriteTestTempDB(connString);
            }

        }



        private void DBPreMigrationCheck(string connString)
        {
            const string sysAdminCommand = "exec [dbo].[p_raptor_run_before_conversion]";
            printNewFeature("Database V6 Check", "", "", "");

            using (var conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(sysAdminCommand, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                printNewFeature(reader[0].ToString(), reader[1].ToString(), "", reader[2].ToString());
                                printFeatureStatus(false);
                            }
                        }
                    }
                    conn.Close();
                }
                catch (Exception ex)
                {
                    printNewFeature("Error", "Database", "", ex.Message.ToString());
                    printFeatureStatus(false);
                }
            }
        }

        private void DBPreMigrationAbsenceTable(string connString)
        {
            string commandText1 = "SELECT [abs_id],[abs_desc],[abs_desc_nonuni],[abs_short_desc],[abs_short_desc_nonuni],[extended_abs],[color_code],[apply_time_rules],[apply_count_rules],[activity_id],[deleted],[time_tracking],[in_worktime],[paid_time],[block_delimiter],[planned_absence],[unplanned_absence],[vacation],[private_desc],[private_color],[changed_by],[changed_date] FROM [dbo].[absences]";
            ArrayList rowList = new ArrayList();


            using (var conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(commandText1, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                object[] values = new object[reader.FieldCount];
                                reader.GetValues(values);
                                rowList.Add(values);
                            }
                        }
                    }

                    listView2.Items.Clear();

                    foreach (object[] row in rowList)
                    {
                        string[] orderDetails = new string[row.Length];
                        int columnIndex = 0;

                        foreach (object column in row)
                        {
                            orderDetails[columnIndex++] = Convert.ToString(column);
                        }

                        ListViewItem newItem = new ListViewItem(orderDetails);
                        listView2.Items.Add(newItem);
                    }

                    conn.Close();
                }
                catch (Exception ex)
                {
                    printNewFeature("Error", "Database", "", ex.Message.ToString());
                    printFeatureStatus(false);
                }
            }



        }

        private void DBPreMigrationActivitiesTable(string connString)
        {
            string commandText1 = "SELECT [activity_id],[activity_name],[activity_name_nonuni],[in_worktime],[color_code],[occupies_workplace],[show_in_schedule],[extended_activity],[in_shiftname],[ep_lunch_break],[ep_short_break],[is_logged],[req_skill],[is_parent],[parent_id],[deleted],[overwrite],[paid_time],[planned_absence],[unplanned_absence],[vacation],[private_desc],[private_color],[changed_by],[changed_date] FROM [dbo].[activities]";
            ArrayList rowList = new ArrayList();


            using (var conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();



                    using (var cmd = new SqlCommand(commandText1, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                object[] values = new object[reader.FieldCount];
                                reader.GetValues(values);
                                rowList.Add(values);
                            }
                        }
                    }

                    listView3.Items.Clear();

                    foreach (object[] row in rowList)
                    {
                        string[] orderDetails = new string[row.Length];
                        int columnIndex = 0;

                        foreach (object column in row)
                        {
                            orderDetails[columnIndex++] = Convert.ToString(column);
                        }

                        ListViewItem newItem = new ListViewItem(orderDetails);
                        listView3.Items.Add(newItem);
                    }

                    conn.Close();
                }
                catch (Exception ex)
                {
                    printNewFeature("Error", "Database", "", ex.Message.ToString());
                    printFeatureStatus(false);
                }
            }



        }

        private static void ReadSingleRow(IDataRecord record)
        {
            Console.WriteLine(String.Format("{0}, {1}", record[0], record[1]));
        }

        private void WriteTestTempDB(string connString)
        {
            string procedureCommand = string.Empty;

            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("^\\s*GO\\s*$", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline);
            // create a writer and open the file
            string filePath = @"WriteTestTempDB.sql";
            using (var conn = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    if (File.Exists(filePath))
                    {
                        StreamReader file = null;
                        try
                        {
                            file = new StreamReader(filePath);
                            string[] lines = regex.Split(file.ReadToEnd());

                            foreach (string line in lines)
                            {
                                if (line.Length > 0)
                                {
                                    cmd.CommandText = line;
                                    cmd.CommandType = CommandType.Text;
                                    SqlDataReader reader = cmd.ExecuteReader();

                                    //bytes to be transfered
                                    reader.Read();
                                    var MBytesRead = reader.GetInt32(0) / 1024 / 1024;

                                    //actual data
                                    var stopwatch = new Stopwatch();
                                    stopwatch.Start();

                                    reader.NextResult();
                                    while (reader.Read()) { }
                                    stopwatch.Stop();
                                    var BandWidth = Math.Round(8 * MBytesRead * 1000 / stopwatch.Elapsed.TotalMilliseconds, 2);

                                    printNewFeature("Database", "Server Lan speed [mbps]", "200", BandWidth.ToString());
                                    printFeatureStatus(true);


                                    //IOLatency figures
                                    reader.NextResult();
                                    reader.Read();

                                    var IoWriteLatency = reader.GetInt64(0);
                                    var displayIoWriteLatency = "<1";

                                    if (IoWriteLatency > 0)
                                        displayIoWriteLatency = IoWriteLatency.ToString();

                                    printNewFeature("Database", "IoWriteLatency [ms]", "5", displayIoWriteLatency);
                                    if (IoWriteLatency > 5)
                                        printFeatureStatus(false);
                                    else
                                        printFeatureStatus(true);

                                    reader.Close();

                                }
                            }

                        }
                        finally
                        {
                            if (file != null)

                                file.Close();
                        }
                    }
                    conn.Close();
                }
            }
        }




        private void InsertProcedure(string connString)
        {
            string procedureCommand = string.Empty;
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("^\\s*GO\\s*$", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline);
            // create a writer and open the file
            string filePath = @"p_raptor_run_before_conversion.sql";
            using (var conn = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    if (File.Exists(filePath))
                    {
                        StreamReader file = null;
                        try
                        {
                            file = new StreamReader(filePath);
                            string[] lines = regex.Split(file.ReadToEnd());

                            foreach (string line in lines)
                            {
                                if (line.Length > 0)
                                {
                                    cmd.CommandText = line;
                                    cmd.CommandType = CommandType.Text;
                                    cmd.NotificationAutoEnlist = true;
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            //while ((line = file.ReadLine()) != null)
                            //{
                            //    if (line.ToString() == "GO")
                            //    {
                            //        cmd.CommandText = procedureCommand;
                            //        res = cmd.ExecuteNonQuery();
                            //        cmd.CommandText = "";
                            //        procedureCommand = "";
                            //    }
                            //    else
                            //    {
                            //        procedureCommand = procedureCommand + @line.ToString() + " ";
                            //    }
                            //}
                            //cmd.CommandText = procedureCommand.Replace("GO","");
                            //res = cmd.ExecuteNonQuery();
                        }
                        finally
                        {
                            if (file != null)

                                file.Close();
                        }
                    }
                    conn.Close();
                }
            }



        }
        private string connStringGet(string dbName)
        {
            //Set the connection string (win or SQL)
            string connStr = "";

            if (radioButtonWinAuth.Checked)
            {
                connStr = string.Format("Application Name=TeleoptiPreReqCheck;Data Source={0};Persist Security Info=True;Integrated Security=SSPI;Initial Catalog=" + dbName,
                    textBoxSQLServerName.Text);
            }
            else
            {
                connStr = string.Format("Application Name=TeleoptiPreReqCheck;Data Source={0};Persist Security Info=True;User Id={1};Password={2};Initial Catalog=" + dbName,
                    textBoxSQLServerName.Text, SQLLogin.Text, SQLPwd.Text);
            }
            return connStr;
        }


        private void DBConnectionCheck(ref bool DbConn, string connString)
        {
            printNewFeature("Database", "DB Connection", "", "Connecting: " + textBoxSQLServerName.Text.ToString());

            using (var conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                        printFeatureStatus(true);
                    conn.Close();
                    DbConn = true;
                }
                catch
                {
                    printFeatureStatus(false);
                }
            }
        }

        private void DBSoftwareCheck()
        {
            string software = "";
            string minValue = "";

            //SQL Management Studio - Used for sql management and development
            software = "Microsoft SQL Server Management Studio";
            minValue = "SQL Management Studio 2005/2008";
            CheckSoftware(software, minValue);

            //SQL BI Studio - Used for SSIS development
            software = "Microsoft SQL Server Development Studio";
            minValue = "SQL BI Studio 2005/2008";
            CheckSoftware(software, minValue);

        }

        private void CheckSoftware(string software, string minValue)
        {
            string[] words = software.Split(' ');

            List<string> softwareList = ListSoftware(".", words);

            if (softwareList.Count > 0)
            {
                for (int i = 0; i < softwareList.Count; i++)
                {
                    printNewFeature("Database", "Software", minValue, softwareList[i].ToString());
                    printFeatureStatus(true);
                }
            }
            else
            {
                printNewFeature("Database", "Software", minValue, software.ToString());
                printFeatureStatus("Check Manually");
            }
        }

        private static List<string> ListSoftware(string Server, string[] Words)
        {

            //The registry key:
            string SoftwareKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

            //Declare the list of strings to get each software match:
            List<string> foundSoftware = new List<string>();

            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(SoftwareKey))
            {
                //Let's go through the registry keys and get the info we need:
                foreach (string skName in rk.GetSubKeyNames())
                {
                    using (RegistryKey sk = rk.OpenSubKey(skName))
                    {
                        try
                        {
                            for (int i = 0; i < Words.Length; i++)
                            {
                                if (!sk.GetValue("DisplayName").ToString().Contains(Words[i]))
                                    break;
                                else //is this the last word?
                                {
                                    if (i == Words.Length - 1)
                                        foundSoftware.Add(sk.GetValue("DisplayName").ToString());
                                }
                            }
                        }
                        catch (Exception)
                        {
                            //MessageBox.Show("Opening regKey failed with exception: " + ex.Message);
                        }
                    }
                }
            }
            //duplicates RegKeys seems to reported for many products. Remove them before return
            foundSoftware = removeDuplicates(foundSoftware);

            return foundSoftware;
        }


        private static string SQLServerCollation(string connString)
        {
            string res;
            const string versionCommand = "select convert(char(200), ServerProperty('Collation'))";

            using (var conn = new SqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(versionCommand, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        res = reader.GetString(0);
                    }
                }
                conn.Close();
            }

            return res;
        }

        private static int SQLServerMajorVersion(string connString)
        {
            string res;
            const string versionCommand = "select convert(char(20), serverproperty('ProductVersion'))";
            using (var conn = new SqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(versionCommand, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        res = reader.GetString(0).Split(new Char[] { '.' })[0];
                    }
                }
                conn.Close();
            }

            return Int32.Parse(res);
        }

        private static string SQLServerFullVersion(string connString)
        {
            string res;
            const string versionCommand = "select convert(char(20), serverproperty('ProductVersion'))";
            using (var conn = new SqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(versionCommand, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        res = reader.GetString(0);
                    }
                }
                conn.Close();
            }

            return res;
        }

        private void DBCollationCheck(string connString)
        {
            string collation = SQLServerCollation(connString);
            printNewFeature("Database", "Collation", "SQL_Latin1_General_CP1_CI_AS", collation);

            if (collation.ToUpper().Contains("_BIN"))  //Binary collation
                printFeatureStatus(false);

            else if (collation.ToUpper().Contains("_CS_")) //Case Sensitive
                printFeatureStatus("Warning");
            else
                printFeatureStatus(true);
        }

        private void DBVersionCheck(string connString)
        {
            int dbMajorVersion = SQLServerMajorVersion(connString);
            string dbFullVersion = SQLServerFullVersion(connString);
            printNewFeature("Database", "Full Version", "MS SQL Server 2005 or later", dbFullVersion.ToString());

            if (dbMajorVersion >= 9)
                printFeatureStatus(true);
            else
                printFeatureStatus(false);
        }

        private void DBSysAdminCheck(string connString)
        {
            const string sysAdminCommand = "SELECT IS_SRVROLEMEMBER('sysadmin')";
            printNewFeature("Database", "Permission", "SysAdmin role required", "Sys Admin role");

            using (var conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(sysAdminCommand, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            reader.Read();
                            if (reader.GetInt32(0) == 1)
                                printFeatureStatus(true);
                            else
                                printFeatureStatus(false);
                        }
                    }
                    conn.Close();
                }
                catch
                {
                    printFeatureStatus(false);
                }
            }
        }
        #endregion

        #region HardWare Check methods
        private void CheckMemory(int numberOfAgent)
        {

            int oneGb = 1073741824;

            try
            {
                ManagementObjectSearcher Search = new ManagementObjectSearcher("Select * From Win32_ComputerSystem");
                foreach (ManagementObject Mobject in Search.Get())
                {
                    double Ram_Bytes = (Convert.ToDouble(Mobject["TotalPhysicalMemory"]));
                    printNewFeature("Hardware", "RAM Size in Giga Bytes", "", Math.Round(Ram_Bytes / oneGb, 1).ToString());

                    if (numberOfAgent < 500 && Math.Round(Ram_Bytes / oneGb, 1) >= 4)
                        printFeatureStatus(true);
                    else if (numberOfAgent <= 1500 && Math.Round(Ram_Bytes / oneGb, 1) >= 8)
                        printFeatureStatus(true);
                    else if (numberOfAgent <= 4000 && Math.Round(Ram_Bytes / oneGb, 1) >= 16)
                        printFeatureStatus(true);
                    else if (numberOfAgent > 4000 && Math.Round(Ram_Bytes / oneGb, 1) >= 32)
                        printFeatureStatus(true);
                    else
                        printFeatureStatus(false);
                }
            }
            catch (Exception ex)
            {
                printNewFeature("Hardware", "RAM Size in Giga Bytes", "", "Error when checking " + ex.Message.ToString());
                printFeatureStatus(false);
            }
        }

        private void CheckArchitecture()
        {
            using (RegistryKey componentsKey =
                Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment", false))
            {
                if (componentsKey != null)
                {
                    string architecture = (string)componentsKey.GetValue("PROCESSOR_ARCHITECTURE", "N/A");
                    printNewFeature("Operating System", "Architecture", "x64 required", architecture);
                    if (architecture.Contains("64") == true)
                        printFeatureStatus(true);
                    else
                        printFeatureStatus(false);

                }
            }
        }

        private void CheckCPUCores(int numberOfAgent)
        {
            using (RegistryKey componentsKey =
                Registry.LocalMachine.OpenSubKey(@"Hardware\Description\System\CentralProcessor", false))
            {
                if (componentsKey != null)
                {
                    int processors = componentsKey.SubKeyCount;
                    printNewFeature("Hardware", "Processor CPU's ", "", processors.ToString());
                    if (numberOfAgent < 500 && processors >= 2)
                        printFeatureStatus(true);
                    else if (numberOfAgent <= 1500 && processors >= 4)
                        printFeatureStatus(true);
                    else if (numberOfAgent <= 4000 && processors >= 8)
                        printFeatureStatus(true);
                    else if (numberOfAgent > 4000 && processors >= 16)
                        printFeatureStatus(true);
                    else
                        printFeatureStatus(false);
                }
            }
        }

        private void CheckOS()
        {
            //See: http://www.sharparena.com/index.php?option=com_content&view=article&id=43:article-csharp-windows-version-edition&catid=15:category-csharp-general&Itemid=12
            //Get OS info

            printNewFeature("Operating System", "OS Version", "Windows Server 2003 R2 or later", SystemInfoApp.SystemInfo.Version.ToString());

            switch (SystemInfoApp.SystemInfo.MajorVersion)
            {
                case 5:
                    if (SystemInfoApp.SystemInfo.MinorVersion == 2) //Win2003:Check for R2!
                    {
                        if (SystemInfoApp.SystemInfo.Version == SystemInfoApp.WindowsVersion.WindowsServer2003R2)
                        {
                            printFeatureStatus(true);
                        }
                        else
                        {
                            printFeatureStatus(false);
                        }
                    }
                    break;
                case 6:
                    if (SystemInfoApp.SystemInfo.Version == SystemInfoApp.WindowsVersion.WindowsServer2008R2
                        || SystemInfoApp.SystemInfo.Version == SystemInfoApp.WindowsVersion.WindowsServer2008)
                    {
                        printFeatureStatus(true);
                    }
                    else
                    {
                        printFeatureStatus(false);
                    }
                    break;
                default:
                    printFeatureStatus(false);
                    break;
            }
        }

        private void CheckComputerName()
        {
            //Get computer name
            string ComputerName = SystemInformation.ComputerName;


            printNewFeature("Operating system", "Computer name", "Allowed characters for computer is [A-Z,a-z,0-9]", ComputerName);

            if (ComputerName == Regex.Replace(ComputerName, "[^a-zA-Z0-9_]", ""))
            {
                printFeatureStatus(true);
            }
            else
            {
                printFeatureStatus(false);
            }

        }


        #endregion

        #region Web Checks methods
        private void CheckNetFx()
        {

            printNewFeature(".NET framework", ".NET Framework", "version 4.0 required", FrameworkVersionDetection.GetExactVersion(FrameworkVersion.Fx40).ToString());
            if (FrameworkVersionDetection.IsInstalled(FrameworkVersion.Fx40))
                printFeatureStatus(true);
            else
                printFeatureStatus(false);
        }

        private void CheckIIS()
        {
            using (RegistryKey componentsKey =
            Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\InetStp", false))
            {
                if (componentsKey != null)
                {

                    int majorVersion = (int)componentsKey.GetValue("MajorVersion", -1);
                    int minorVersion = (int)componentsKey.GetValue("MinorVersion", -1);
                    if (majorVersion != -1 && minorVersion != -1)
                    {
                        printNewFeature("IIS installed", "IIS version", "IIS 5.1 or later", majorVersion + "." + minorVersion);
                        printFeatureStatus(true);
                    }
                    if (majorVersion == 7 || majorVersion == 8)
                    {
                        CheckIIS7SubComponents();
                    }
                    else //assume 6 or 5.1
                    {
                        CheckIIS6SubComponents();
                    }
                }
                else
                {
                    printNewFeature("IIS installed", "IIS web server is not installed", "IIS 5.1 or later", "");
                    printFeatureStatus(false);
                }
            }
        }

        private void CheckIIS7SubComponents()
        {
            using (RegistryKey componentsKey =
            Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\InetStp\Components", false))
            {
                if (componentsKey != null)
                {
                    printNewFeature("IIS Subcomp", "IIS Management Console", "", "");
                    if (InternetInformationServicesDetection.IsInstalled(InternetInformationServices7Component.ManagementConsole))
                        printFeatureStatus(true);
                    else
                        printFeatureStatus(false);

                    printNewFeature("IIS Subcomp", "ASP.NET registered", "", "");
                    if (InternetInformationServicesDetection.IsInstalled(InternetInformationServices7Component.ASPNET))
                        printFeatureStatus(true);
                    else
                        printFeatureStatus(false);

                    printNewFeature("IIS Subcomp", ".NET Extensibility", "", "");
                    if (InternetInformationServicesDetection.IsInstalled(InternetInformationServices7Component.NetFxExtensibility))
                        printFeatureStatus(true);
                    else
                        printFeatureStatus(false);

                    printNewFeature("IIS Subcomp", "Static Content", "", "");
                    if (InternetInformationServicesDetection.IsInstalled(InternetInformationServices7Component.StaticContent))
                        printFeatureStatus(true);
                    else
                        printFeatureStatus(false);

                    printNewFeature("IIS Subcomp", "Basic authentication", "", "");
                    if (InternetInformationServicesDetection.IsInstalled(InternetInformationServices7Component.BasicAuthentication))
                        printFeatureStatus(true);
                    else
                        printFeatureStatus(false);

                    printNewFeature("IIS Subcomp", "Windows authentication", "", "");
                    if (InternetInformationServicesDetection.IsInstalled(InternetInformationServices7Component.WindowsAuthentication))
                        printFeatureStatus(true);
                    else
                        printFeatureStatus(false);


                    try
                    {

                        printNewFeature("IIS Subcomp", ".NET 4.0 registered", "", "");
                        if (InternetInformationServicesDetection.IsAspNetRegistered(FrameworkVersion.Fx40))
                            printFeatureStatus(true);
                        else
                            printFeatureStatus(false);

                    }
                    catch (Exception ex)
                    {
                        printNewFeature("IIS Subcomp", ".NET 4.0 registered", "", "Error when checking" + ex.Message.ToString());
                        printFeatureStatus(false);
                    }

                    try
                    {

                        printNewFeature("IIS .WCF", "WCF registered", "", "");
                        if (FrameworkVersionDetection.IsInstalled(WindowsFoundationLibrary.WCF))
                            printFeatureStatus("Check Manually");
                        //else
                        //    printFeatureStatus(false);
                    }
                    catch (Exception ex)
                    {
                        printNewFeature("IIS .WCF", "WCF registered", "", "Error when checking" + ex.Message.ToString());
                        printFeatureStatus(false);
                    }
                }
            }

            try
            {
                using (DirectoryEntry de = new DirectoryEntry("IIS://localhost/W3SVC"))
                {
                    printNewFeature("IIS .Net", "ASP.NET Enabled", "", "");
                    foreach (string ext in de.Properties["WebSvcExtRestrictionList"])
                    {
                        if (ext.IndexOf("ASP.NET v2.0") != -1)
                        {
                            if (ext.StartsWith("1,"))
                                printFeatureStatus(true);
                            else
                                printFeatureStatus(false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                printNewFeature("IIS .Net", "ASP.NET Enabled", "", "Error when checking" + ex.Message.ToString());
                printFeatureStatus(false);
            }

        }

        private void CheckIIS6SubComponents()
        {
            using (RegistryKey componentsKey =
            Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Setup\Oc Manager\Subcomponents", false))
            {
                if (componentsKey != null)
                {
                    printNewFeature("IIS", "IIS Management Console", "", "");
                    if (InternetInformationServicesDetection.IsInstalled(InternetInformationServicesComponent.InetMgr))
                        printFeatureStatus(true);
                    else
                        printFeatureStatus(false);

                    //asp.net detection only for 32bit
                    //printNewFeature("IIS", "ASP.NET registered", "");
                    //if (InternetInformationServicesDetection.IsInstalled(InternetInformationServicesComponent.AspNet))
                    //    printFeatureStatus(true);
                    //else
                    //    printFeatureStatus(false);

                }
            }

            const string WebServerSchema = "IIsWebServer"; // Case Sensitive
            string ServerName = "LocalHost";
            DirectoryEntry W3SVC = new DirectoryEntry("IIS://" + ServerName + "/w3svc/1/ROOT");
            //DirectoryEntry Site = W3SVC.Children[WebServerSchema];
            foreach (DirectoryEntry Site in W3SVC.Children)
            {
                if (Site.SchemaClassName == WebServerSchema)
                {
                    //printNewFeature("IIS", "WindowsAuthentication", Site.Properties["AuthFlags"].Value.ToString());
                    //    //if (Site.Properties["NTAuthenticationProviders"].Value.ToString().Contains("NTLM") == true)
                    //    if (Site.Properties["AuthFlags"].Value.ToString().Contains("NTLM") == true)
                    //        printFeatureStatus(true);
                    //    else
                    //        printFeatureStatus(false);

                    printNewFeature("IIS", "ASP.NET Registered", "", "");
                    try
                    {
                        if (Site.Properties["HttpCustomHeaders"].Value.ToString() == "X-Powered-By: ASP.NET")
                            printFeatureStatus(true);
                        else
                            printFeatureStatus(false);

                    }
                    catch
                    {
                        //We will assume that the feature is not installed.
                        printFeatureStatus(false);
                    }
                    break;
                }
            }

            using (DirectoryEntry de = new DirectoryEntry("IIS://localhost/W3SVC"))
            {
                printNewFeature("IIS", "ASP.NET Enabled", "", "");
                foreach (string ext in de.Properties["WebSvcExtRestrictionList"])
                {
                    if (ext.IndexOf("ASP.NET v2.0") != -1)
                    {
                        if (ext.StartsWith("1,"))
                            printFeatureStatus(true);
                        else
                            printFeatureStatus(false);
                    }
                }
            }

        }

        #endregion

        private void ServiceCheck(string Target, string ServiceName)
        {
            //print something that people might recongise for SSIS
            if (ServiceName.Contains("MSDTSSERVER"))
                printNewFeature("SSIS Service", "Service", "Running", "");
            else
                printNewFeature(ServiceName.ToString(), "Service", "Running", "");

            listView1.Items[listView1.Items.Count - 1].SubItems[2].Text = CheckServiceRunning(Target.ToString(), ServiceName.ToString()).ToString();
            if (CheckServiceRunning(Target.ToString(), ServiceName.ToString()).ToString() == "Running")
            {


                printFeatureStatus(true);
            }
            else
            {
                printFeatureStatus(false);

            }
        }

        private void CheckDatabaseServices()
        {

            //Initialise as a default instance
            string DBServer = comboBoxSQLInstance.SelectedItem.ToString().Trim();
            string DBInstance = "MSSQLSERVER";

            string SQLEngineService = "MSSQLSERVER";
            string SQLAgentService = "SQLSERVERAGENT";
            string OLAPService = "MSSQLServerOLAPService";
            string SSIS = "MSDTSSERVER";
            double SQLVersion = double.Parse(SQLServerFullVersion(connStringGet("master")).Substring(0, 4), CultureInfo.InvariantCulture);

            if (SQLVersion > 10)
                SSIS = "MSDTSSERVER100";

            //Next, find out if this is a named instance
            //int index = comboBoxSQLInstance.SelectedItem.ToString().IndexOf(@"\");
            int index = DBServer.ToString().IndexOf(@"\");
            int len = DBServer.ToString().Length;

            //If namned instance
            if (index != -1)
            {

                DBInstance = DBServer.ToString().Substring(index + 1, len - index - 1);
                DBServer = DBServer.ToString().Substring(0, index);

                SQLEngineService = "MSSQL$" + DBInstance;
                SQLAgentService = "SQLAGENT$" + DBInstance;
                OLAPService = "MSOLAP$" + DBInstance;
                //SSIS = "MSDTSSERVER"; //does not use multiple instances

            }

            //Check each needed service
            if (DBServer != "NoInstanceDetected")
            {
                ServiceCheck(DBServer, SQLEngineService);
                ServiceCheck(DBServer, SQLAgentService);
                ServiceCheck(DBServer, OLAPService);
                ServiceCheck(DBServer, SSIS);
            }
        }





        private static string CheckServiceRunning(string Server, string ServiceName)
        {
            string ServiceStatus = "";
            try
            {
                ConnectionOptions connection = new ConnectionOptions();
                ManagementScope scope = new ManagementScope("\\\\" + Server + "\\root\\CIMV2");
                scope.Connect();
                ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_Service WHERE Name = '" + ServiceName + "'");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
                if (searcher.Get().Count == 0)
                {
                    ServiceStatus = "Does not exist";
                }

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if (queryObj["State"].ToString() == "Running")
                    {
                        ServiceStatus = "Running".ToString();
                    }
                    else
                        ServiceStatus = queryObj["State"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wmi could not get service: " + ServiceName.ToString() + ", With exception: " + ex);
                ServiceStatus = "WMI failed to get: " + ServiceName.ToString();
            }
            return ServiceStatus;
        }

        #region PrintStuff


        private void printNewFeature(string checkWhat, string feature, string minValue, string featureValue)
        {
            listView1.Items.Add(checkWhat.ToString());
            listView1.Items[listView1.Items.Count - 1].SubItems.Add(feature.ToString());
            listView1.Items[listView1.Items.Count - 1].SubItems.Add(featureValue.ToString());
            listView1.Items[listView1.Items.Count - 1].SubItems.Add(minValue.ToString());
        }

        private void printSubFeature2(string featureValue)
        {
            listView1.Items[listView1.Items.Count - 1].SubItems.Add(featureValue.ToString());
        }


        private void printFeatureStatus(bool featureStatus)
        {
            if (featureStatus)
            {
                listView1.Items[listView1.Items.Count - 1].SubItems.Add("OK");
                listView1.Items[listView1.Items.Count - 1].ForeColor = Color.Green;
            }
            else
            {
                listView1.Items[listView1.Items.Count - 1].SubItems.Add("not OK!");
                listView1.Items[listView1.Items.Count - 1].ForeColor = Color.Red;
            }
        }

        private void printFeatureStatus(string status)
        {
            if (status == "Warning")
            {
                listView1.Items[listView1.Items.Count - 1].SubItems.Add("Warning");
                listView1.Items[listView1.Items.Count - 1].ForeColor = Color.Orange;
            }
            else
            {
                listView1.Items[listView1.Items.Count - 1].SubItems.Add(status);
                listView1.Items[listView1.Items.Count - 1].ForeColor = Color.Black;
            }
        }
        #endregion

        #region helper
        //enumerate all local SQL instances
        void SQLInstanceEnum()
        {
            comboBoxSQLInstance.Items.Clear();

            RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server");
            if (rk != null)
            {
                String[] instances = (String[])rk.GetValue("InstalledInstances");
                if (instances != null)
                {
                    if (instances.Length > 0)
                    {
                        foreach (String element in instances)
                        {
                            if (element == "MSSQLSERVER")
                                comboBoxSQLInstance.Items.Add(System.Environment.MachineName);
                            else
                                comboBoxSQLInstance.Items.Add(System.Environment.MachineName + @"\" + element);
                        }
                    }
                }
            }
            else
                comboBoxSQLInstance.Items.Add("NoInstanceDetected");
        }

        //If shared web+Db server then get a higher number of agents
        int AgentLevel()
        {
            int numberOfAgent = (int)numericUpDownAgents.Value;

            //Check if the server is intended to be both web AND db
            if (comboBoxServerSetup.SelectedItem.ToString().Contains("Web")
                && comboBoxServerSetup.SelectedItem.ToString().Contains("DB"))
            {
                if (numberOfAgent < 500)
                    numberOfAgent = numberOfAgent + 1500;
                else if (numberOfAgent <= 4000)
                    numberOfAgent = numberOfAgent + 4000;
                else if (numberOfAgent > 4000)
                    MessageBox.Show("Warning! Running more then 4000 agents on a single server is not supported");
            }
            return numberOfAgent;
        }


        //remove duplicates from a list of strings
        private static List<string> removeDuplicates(List<string> input)
        {
            Dictionary<string, int> tmp = new Dictionary<string, int>();

            foreach (string s in input)
                tmp[s] = 1;

            return new List<string>(tmp.Keys);
        }

        private void CopyListViewToClipboard(ListView lv)
        {
            StringBuilder buffer = new StringBuilder();

            foreach (ColumnHeader header in lv.Columns)
            {
                buffer.Append(header.Text.Trim());
                buffer.Append('\t');
            }
            buffer.Append(Environment.NewLine);

            foreach (ListViewItem item in lv.Items)
            {
                foreach (ListViewItem.ListViewSubItem subItem in item.SubItems)
                {
                    buffer.Append(subItem.Text);
                    buffer.Append('\t');
                }
                buffer.Append(Environment.NewLine);
            }

            Clipboard.SetText(buffer.ToString().Trim());
        }
        private void CopyAllListViewToClipboard()
        {
            StringBuilder buffer = new StringBuilder();

            foreach (ColumnHeader header in listView1.Columns)
            {
                buffer.Append(header.Text.Trim());
                buffer.Append('\t');
            }
            buffer.Append(Environment.NewLine);

            foreach (ListViewItem item in listView1.Items)
            {
                foreach (ListViewItem.ListViewSubItem subItem in item.SubItems)
                {
                    buffer.Append(subItem.Text);
                    buffer.Append('\t');
                }
                buffer.Append(Environment.NewLine);
            }

            buffer.Append(Environment.NewLine);
            buffer.Append(Environment.NewLine);

            foreach (ColumnHeader header in listView2.Columns)
            {
                buffer.Append(header.Text.Trim());
                buffer.Append('\t');
            }
            buffer.Append(Environment.NewLine);

            foreach (ListViewItem item in listView2.Items)
            {
                foreach (ListViewItem.ListViewSubItem subItem in item.SubItems)
                {
                    buffer.Append(subItem.Text);
                    buffer.Append('\t');
                }
                buffer.Append(Environment.NewLine);
            }


            buffer.Append(Environment.NewLine);
            buffer.Append(Environment.NewLine);

            foreach (ColumnHeader header in listView3.Columns)
            {
                buffer.Append(header.Text.Trim());
                buffer.Append('\t');
            }
            buffer.Append(Environment.NewLine);

            foreach (ListViewItem item in listView3.Items)
            {
                foreach (ListViewItem.ListViewSubItem subItem in item.SubItems)
                {
                    buffer.Append(subItem.Text);
                    buffer.Append('\t');
                }
                buffer.Append(Environment.NewLine);
            }


            Clipboard.SetText(buffer.ToString().Trim());
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(listView1, e.Location);
            }
        }

        private void contextMenuStrip1_Click(object sender, EventArgs e)
        {
            if (comboBoxServerSetup.SelectedItem.ToString().Contains("Version6"))
            {
                CopyAllListViewToClipboard();
            }
            else
            {
                CopyListViewToClipboard(listView1);
            }

        }
        #endregion

        #region GUI events
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            SQLLogin.Enabled = true;
            SQLPwd.Enabled = true;
            SQLLogin.Text = "";
        }

        private void radioButtonWniAuth(object sender, EventArgs e)
        {
            SQLLogin.Enabled = false;
            SQLPwd.Enabled = false;
            SQLLogin.Text = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBoxServerSetup.SelectedIndex = 0;
            groupBoxSQLInstance.Hide();
            SQLLogin.Text = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        }

        private void comboBoxServerSetup_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxServerSetup.SelectedItem.ToString().Contains("DB"))
            {
                groupBoxSQLInstance.Show();
                SQLInstanceEnum();
                comboBoxSQLInstance.SelectedIndex = 0;
                groupBoxDatabase.Hide();
            }
            else if (comboBoxServerSetup.SelectedItem.ToString().Contains("Version6"))
            {
                groupBoxDatabase.Show();
                groupBoxSQLInstance.Hide();
            }
            else
            {
                comboBoxSQLInstance.Items.Clear();
                groupBoxSQLInstance.Hide();
                groupBoxDatabase.Hide();
            }
        }
        private void comboBoxSQLInstance_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxSQLServerName.Text = comboBoxSQLInstance.SelectedItem.ToString();
        }

        #endregion

        private void ClearListView()
        {
            listView1.Items.Clear();
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

    }
}