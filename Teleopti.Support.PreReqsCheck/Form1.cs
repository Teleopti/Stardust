using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.DirectoryServices;
using Microsoft.Win32;
using System.Management;
using Campari.Software;
using System.IO;
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
		// ReSharper disable InconsistentNaming
		private void button1_Click(object sender, EventArgs e)
		{
			ClearListView();

			//Check web and/or DB server
			if (comboBoxServerSetup.SelectedItem.ToString().Contains("Web"))
			{
				RunHardWareChecks((int) numericUpDownAgents.Value);
				RunWebChecks(AgentLevel());
			}
			if (comboBoxServerSetup.SelectedItem.ToString().Contains("DB"))
			{
				RunHardWareChecks((int) numericUpDownAgents.Value);
				RunDbChecks();
			}
			
			if (comboBoxServerSetup.SelectedItem.ToString().Contains("Version6"))
				RunV6Checks();
		}

		private void CheckDbConnection_Click(object sender, EventArgs e)
		{
			CheckDbInternals();
		}
		#endregion

		#region V6 Checks
		private void RunV6Checks()
		{
			var dbConnection = false;
			var connectionString = ConnStringGet(textBoxDBName.Text);

			//Check that it's possible to connect
			DBConnectionCheck(ref dbConnection, connectionString);

			//if we have a Db-connection, go for the DB-checks
			if (!dbConnection) return;
			InsertProcedure(connectionString);
			DbPreMigrationCheck(connectionString);
			DbPreMigrationAbsenceTable(connectionString);
			DbPreMigrationActivitiesTable(connectionString);
		}
		#endregion

		#region DB Checks
		private void RunDbChecks()
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
		private void RunWebChecks(int agentLevel)
		{
			//Check .net framework installed
			CheckNetFx();

			//Check IIS exists and sub components installed
			CheckIIS();
		}
		#endregion

		#region DB Checks methods
		private void CheckDbInternals()
		{
			var connectionString = ConnStringGet("master");
			var dbConnection = false;

			//Check that it's possible to connect
			DBConnectionCheck(ref dbConnection, connectionString);

			//if we have a Db-connection, go for the DB-checks
			if (!dbConnection) return;

			//Check SQL Server Version
			DBVersionCheck(connectionString);

			//Check we have sysadmin role
			DBSysAdminCheck(connectionString);

			//Collation
			DBCollationCheck(connectionString);

			//WriteTestTempDB
			WriteTestTempDb(connectionString);
		}

		private void DbPreMigrationCheck(string connString)
		{
			const string sysAdminCommand = "exec [dbo].[p_raptor_run_before_conversion]";
			printNewFeature("Database V6 Check", "", "", "");

			using (var conn = new SqlConnection(connString))
			{
				try
				{
					conn.Open();
					using (var cmd = new SqlCommand(sysAdminCommand, conn))
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							printNewFeature(reader[0].ToString(), reader[1].ToString(), "", reader[2].ToString());
							printFeatureStatus(false);
						}
					}
					conn.Close();
				}
				catch (Exception ex)
				{
					printNewFeature("Error", "Database", "", ex.Message);
					printFeatureStatus(false);
				}
			}
		}

		private void DbPreMigrationAbsenceTable(string connString)
		{
			const string commandText1 =
				"SELECT [abs_id],[abs_desc],[abs_desc_nonuni],[abs_short_desc],[abs_short_desc_nonuni],[extended_abs],[color_code],[apply_time_rules],[apply_count_rules],[activity_id],[deleted],[time_tracking],[in_worktime],[paid_time],[block_delimiter],[planned_absence],[unplanned_absence],[vacation],[private_desc],[private_color],[changed_by],[changed_date] FROM [dbo].[absences]";
			var rowList = new ArrayList();

			using (var conn = new SqlConnection(connString))
			{
				try
				{
					conn.Open();
					using (var cmd = new SqlCommand(commandText1, conn))
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var values = new object[reader.FieldCount];
							reader.GetValues(values);
							rowList.Add(values);
						}
					}

					listView2.Items.Clear();
					foreach (object[] row in rowList)
					{
						var orderDetails = new string[row.Length];
						var columnIndex = 0;

						foreach (var column in row)
						{
							orderDetails[columnIndex++] = Convert.ToString(column);
						}

						var newItem = new ListViewItem(orderDetails);
						listView2.Items.Add(newItem);
					}
					conn.Close();
				}
				catch (Exception ex)
				{
					printNewFeature("Error", "Database", "", ex.Message);
					printFeatureStatus(false);
				}
			}
		}

		private void DbPreMigrationActivitiesTable(string connString)
		{
			const string commandText1 =
				"SELECT [activity_id],[activity_name],[activity_name_nonuni],[in_worktime],[color_code],[occupies_workplace],[show_in_schedule],[extended_activity],[in_shiftname],[ep_lunch_break],[ep_short_break],[is_logged],[req_skill],[is_parent],[parent_id],[deleted],[overwrite],[paid_time],[planned_absence],[unplanned_absence],[vacation],[private_desc],[private_color],[changed_by],[changed_date] FROM [dbo].[activities]";
			var rowList = new ArrayList();

			using (var conn = new SqlConnection(connString))
			{
				try
				{
					conn.Open();
					using (var cmd = new SqlCommand(commandText1, conn))
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var values = new object[reader.FieldCount];
							reader.GetValues(values);
							rowList.Add(values);
						}
					}

					listView3.Items.Clear();
					foreach (object[] row in rowList)
					{
						var orderDetails = new string[row.Length];
						var columnIndex = 0;

						foreach (var column in row)
							orderDetails[columnIndex++] = Convert.ToString(column);

						var newItem = new ListViewItem(orderDetails);
						listView3.Items.Add(newItem);
					}
					conn.Close();
				}
				catch (Exception ex)
				{
					printNewFeature("Error", "Database", "", ex.Message);
					printFeatureStatus(false);
				}
			}
		}

		private void WriteTestTempDb(string connString)
		{
			var regex = new Regex("^\\s*GO\\s*$",
			                      System.Text.RegularExpressions.RegexOptions.IgnoreCase |
			                      System.Text.RegularExpressions.RegexOptions.Multiline);
			// create a writer and open the file
			const string filePath = @"WriteTestTempDB.sql";
			using (var conn = new SqlConnection(connString))
			using (var cmd = new SqlCommand())
			{
				conn.Open();
				cmd.Connection = conn;
				if (File.Exists(filePath))
				{
					StreamReader file = null;
					try
					{
						file = new StreamReader(filePath);
						var lines = regex.Split(file.ReadToEnd());

						foreach (var line in lines)
						{
							if (line.Length <= 0) 
								continue;

							cmd.CommandText = line;
							cmd.CommandType = CommandType.Text;
							var reader = cmd.ExecuteReader();

							//bytes to be transfered
							reader.Read();
							var mBytesRead = reader.GetInt32(0)/1024/1024;

							//actual data
							var stopwatch = new Stopwatch();
							stopwatch.Start();

							reader.NextResult();
							while (reader.Read())
							{
							}
							stopwatch.Stop();
							var bandWidth = Math.Round(8*mBytesRead*1000/stopwatch.Elapsed.TotalMilliseconds, 2);

							printNewFeature("Database", "Server Lan speed [mbps]", "200", bandWidth.ToString(CultureInfo.InvariantCulture));
							printFeatureStatus(true);


							//IOLatency figures
							reader.NextResult();
							reader.Read();

							var ioWriteLatency = reader.GetInt64(0);
							var displayIoWriteLatency = "<1";

							if (ioWriteLatency > 0)
								displayIoWriteLatency = ioWriteLatency.ToString(CultureInfo.InvariantCulture);

							printNewFeature("Database", "IoWriteLatency [ms]", "5", displayIoWriteLatency);
							printFeatureStatus(ioWriteLatency <= 5);

							reader.Close();
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

		private static void InsertProcedure(string connString)
		{
			var regex = new Regex("^\\s*GO\\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
			// create a writer and open the file
			const string filePath = @"p_raptor_run_before_conversion.sql";
			using (var conn = new SqlConnection(connString))
			using (var cmd = new SqlCommand())
			{
				conn.Open();
				cmd.Connection = conn;
				if (File.Exists(filePath))
				{
					StreamReader file = null;
					try
					{
						file = new StreamReader(filePath);
						var lines = regex.Split(file.ReadToEnd());

						foreach (var line in lines)
						{
							if (line.Length <= 0) continue;

							cmd.CommandText = line;
							cmd.CommandType = CommandType.Text;
							cmd.NotificationAutoEnlist = true;
							cmd.ExecuteNonQuery();
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

		private string ConnStringGet(string dbName)
		{
			//Set the connection string (win or SQL)
			string connStr;

			if (radioButtonWinAuth.Checked)
			{
				connStr =
					string.Format(
						"Application Name=TeleoptiPreReqCheck;Data Source={0};Persist Security Info=True;Integrated Security=SSPI;Initial Catalog=" +
						dbName,
						textBoxSQLServerName.Text);
			}
			else
			{
				connStr =
					string.Format(
						"Application Name=TeleoptiPreReqCheck;Data Source={0};Persist Security Info=True;User Id={1};Password={2};Initial Catalog=" +
						dbName,
						textBoxSQLServerName.Text, SQLLogin.Text, SQLPwd.Text);
			}
			return connStr;
		}

		private void DBConnectionCheck(ref bool dbConn, string connString)
		{
			printNewFeature("Database", "DB Connection", "", "Connecting: " + textBoxSQLServerName.Text);

			using (var conn = new SqlConnection(connString))
			{
				try
				{
					conn.Open();
					if (conn.State == ConnectionState.Open)
						printFeatureStatus(true);
					conn.Close();
					dbConn = true;
				}
				catch
				{
					printFeatureStatus(false);
				}
			}
		}

		private void DBSoftwareCheck()
		{
			//SQL Management Studio - Used for sql management and development
			var software = "Microsoft SQL Server Management Studio";
			var minValue = "SQL Management Studio 2005/2008";
			CheckSoftware(software, minValue);

			//SQL BI Studio - Used for SSIS development
			software = "Microsoft SQL Server Development Studio";
			minValue = "SQL BI Studio 2005/2008";
			CheckSoftware(software, minValue);

		}

		private void CheckSoftware(string software, string minValue)
		{
			var words = software.Split(' ');
			var softwareList = ListSoftware(words);

			if (softwareList.Count > 0)
			{
				for (var i = 0; i < softwareList.Count; i++)
				{
					printNewFeature("Database", "Software", minValue, softwareList[i]);
					printFeatureStatus(true);
				}
			}
			else
			{
				printNewFeature("Database", "Software", minValue, software);
				printFeatureStatus("Check Manually");
			}
		}

		private static List<string> ListSoftware(IList<string> Words)
		{
			//The registry key:
			const string softwareKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

			//Declare the list of strings to get each software match:
			var foundSoftware = new List<string>();

			using (var rk = Registry.LocalMachine.OpenSubKey(softwareKey))
			{
				//Let's go through the registry keys and get the info we need:
				if (rk != null)
					foreach (var skName in rk.GetSubKeyNames())
					{
						using (var sk = rk.OpenSubKey(skName))
						{
							try
							{
								for (var i = 0; i < Words.Count; i++)
								{
									if (sk == null) break;
									if (!sk.GetValue("DisplayName").ToString().Contains(Words[i]))
										break;
									if (i == Words.Count - 1)
										foundSoftware.Add(sk.GetValue("DisplayName").ToString());
								}
							}
							catch (Exception)
							{
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
			using (var cmd = new SqlCommand(versionCommand, conn))
			{
				conn.Open();
				using (var reader = cmd.ExecuteReader())
				{
					reader.Read();
					res = reader.GetString(0);
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
				using (var reader = cmd.ExecuteReader())
				{
					reader.Read();
					res = reader.GetString(0).Split(new[] {'.'})[0];
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
				using (var reader = cmd.ExecuteReader())
				{
					reader.Read();
					res = reader.GetString(0);
				}
				conn.Close();
			}
			return res;
		}

		private void DBCollationCheck(string connString)
		{
			var collation = SQLServerCollation(connString);
			printNewFeature("Database", "Collation", "SQL_Latin1_General_CP1_CI_AS", collation);

			if (collation.ToUpper().Contains("_BIN")) //Binary collation
				printFeatureStatus(false);

			else if (collation.ToUpper().Contains("_CS_")) //Case Sensitive
				printFeatureStatus("Warning");
			else
				printFeatureStatus(true);
		}

		private void DBVersionCheck(string connString)
		{
			var dbMajorVersion = SQLServerMajorVersion(connString);
			var dbFullVersion = SQLServerFullVersion(connString);

			printNewFeature("Database", "Full Version", "MS SQL Server 2005 or later", dbFullVersion);
			printFeatureStatus(dbMajorVersion >= 9);
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
					using (var reader = cmd.ExecuteReader())
					{
						reader.Read();
						printFeatureStatus(reader.GetInt32(0) == 1);
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
			const int oneGb = 1073741824;

			try
			{
				var search = new ManagementObjectSearcher("Select * From Win32_ComputerSystem");
				foreach (var mobject in search.Get())
				{
					var ramBytes = (Convert.ToDouble(mobject["TotalPhysicalMemory"]));
					printNewFeature("Hardware", "RAM Size in Giga Bytes", "",
					                Math.Round(ramBytes/oneGb, 1).ToString(CultureInfo.InvariantCulture));

					if (numberOfAgent < 500 && Math.Round(ramBytes/oneGb, 1) > 4)
						printFeatureStatus(true);
					else if (numberOfAgent <= 1500 && Math.Round(ramBytes/oneGb, 1) > 8)
						printFeatureStatus(true);
					else if (numberOfAgent <= 4000 && Math.Round(ramBytes/oneGb, 1) > 16)
						printFeatureStatus(true);
					else if (numberOfAgent > 4000 && Math.Round(ramBytes/oneGb, 1) > 32)
						printFeatureStatus(true);
					else
						printFeatureStatus(false);
				}
			}
			catch (Exception ex)
			{
				printNewFeature("Hardware", "RAM Size in Giga Bytes", "", "Error when checking " + ex.Message);
				printFeatureStatus(false);
			}
		}

		private void CheckArchitecture()
		{
			using (var componentsKey =
				Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment", false))
			{
				if (componentsKey == null) 
					return;

				var architecture = (string) componentsKey.GetValue("PROCESSOR_ARCHITECTURE", "N/A");
				printNewFeature("Operating System", "Architecture", "x64 required", architecture);
				printFeatureStatus(architecture.Contains("64"));
			}
		}

		private void CheckCPUCores(int numberOfAgent)
		{
			using (var componentsKey =
				Registry.LocalMachine.OpenSubKey(@"Hardware\Description\System\CentralProcessor", false))
			{
				if (componentsKey == null) 
					return;

				var processors = componentsKey.SubKeyCount;
				printNewFeature("Hardware", "Processor CPU's ", "", processors.ToString(CultureInfo.InvariantCulture));
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

		private void CheckOS()
		{
			//See: http://www.sharparena.com/index.php?option=com_content&view=article&id=43:article-csharp-windows-version-edition&catid=15:category-csharp-general&Itemid=12
			//Get OS info

			printNewFeature("Operating System", "OS Version", "Windows Server 2003 R2 or later",
			                SystemInfoApp.SystemInfo.Version.ToString());

			switch (SystemInfoApp.SystemInfo.MajorVersion)
			{
				case 5:
					if (SystemInfoApp.SystemInfo.MinorVersion == 2) //Win2003:Check for R2!
						printFeatureStatus(SystemInfoApp.SystemInfo.Version ==
						                   SystemInfoApp.WindowsVersion.WindowsServer2003R2);
					break;
				case 6:
					if (SystemInfoApp.SystemInfo.Version == SystemInfoApp.WindowsVersion.WindowsServer2008R2
					    || SystemInfoApp.SystemInfo.Version == SystemInfoApp.WindowsVersion.WindowsServer2008)
						printFeatureStatus(true);
					else
						printFeatureStatus(false);
					break;
				default:
					printFeatureStatus(false);
					break;
			}
		}

		private void CheckComputerName()
		{
			var computerName = SystemInformation.ComputerName;
			printNewFeature("Operating system", "Computer name", "Allowed characters for computer is [A-Z,a-z,0-9]", computerName);
			printFeatureStatus(computerName == Regex.Replace(computerName, "[^a-zA-Z0-9_]", ""));
		}
		#endregion

		#region Web Checks methods
		private void CheckNetFx()
		{
			printNewFeature(".NET framework", ".NET Framework", "version 4.0 required",
			                FrameworkVersionDetection.GetExactVersion(FrameworkVersion.Fx40).ToString());
			printFeatureStatus(FrameworkVersionDetection.IsInstalled(FrameworkVersion.Fx40));
		}

		private void CheckIIS()
		{
			using (var componentsKey =
				Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\InetStp", false))
			{
				if (componentsKey != null)
				{
					var majorVersion = (int) componentsKey.GetValue("MajorVersion", -1);
					var minorVersion = (int) componentsKey.GetValue("MinorVersion", -1);
					if (majorVersion != -1 && minorVersion != -1)
					{
						printNewFeature("IIS installed", "IIS version", "IIS 5.1 or later", majorVersion + "." + minorVersion);
						printFeatureStatus(true);
					}
					if (majorVersion == 7 || majorVersion == 8)
						CheckIIS7SubComponents();

					else //assume 6 or 5.1
						CheckIIS6SubComponents();
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
			using (var componentsKey =
				Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\InetStp\Components", false))
			{
				if (componentsKey != null)
				{
					printNewFeature("IIS Subcomp", "IIS Management Console", "", "");
					printFeatureStatus(
						InternetInformationServicesDetection.IsInstalled(
							InternetInformationServices7Component.ManagementConsole));

					printNewFeature("IIS Subcomp", "ASP.NET registered", "", "");
					printFeatureStatus(
						InternetInformationServicesDetection.IsInstalled(InternetInformationServices7Component.ASPNET));

					printNewFeature("IIS Subcomp", ".NET Extensibility", "", "");
					printFeatureStatus(
						InternetInformationServicesDetection.IsInstalled(
							InternetInformationServices7Component.NetFxExtensibility));

					printNewFeature("IIS Subcomp", "Static Content", "", "");
					printFeatureStatus(
						InternetInformationServicesDetection.IsInstalled(InternetInformationServices7Component.StaticContent));

					printNewFeature("IIS Subcomp", "Basic authentication", "", "");
					printFeatureStatus(
						InternetInformationServicesDetection.IsInstalled(
							InternetInformationServices7Component.BasicAuthentication));

					printNewFeature("IIS Subcomp", "Windows authentication", "", "");
					printFeatureStatus(
						InternetInformationServicesDetection.IsInstalled(
							InternetInformationServices7Component.WindowsAuthentication));

					try
					{
						printNewFeature("IIS Subcomp", ".NET 4.0 registered", "", "");
						printFeatureStatus(InternetInformationServicesDetection.IsAspNetRegistered(FrameworkVersion.Fx40));
					}
					catch (Exception ex)
					{
						printNewFeature("IIS Subcomp", ".NET 4.0 registered", "", "Error when checking" + ex.Message);
						printFeatureStatus(false);
					}

					try
					{
						printNewFeature("IIS .WCF", "WCF registered", "", "");
						if (FrameworkVersionDetection.IsInstalled(WindowsFoundationLibrary.WCF))
							printFeatureStatus("Check Manually");
					}
					catch (Exception ex)
					{
						printNewFeature("IIS .WCF", "WCF registered", "", "Error when checking" + ex.Message);
						printFeatureStatus(false);
					}
				}
			}

			try
			{
				using (var de = new DirectoryEntry("IIS://localhost/w3svc"))
				{
					foreach (string ext in de.Properties["WebSvcExtRestrictionList"])
						if (ext.IndexOf("ASP.NET v2.0", StringComparison.Ordinal) != -1)
							printFeatureStatus(ext.StartsWith("1,"));
				}
			}
			catch (Exception ex)
			{
				printNewFeature("IIS .Net", "ASP.NET Enabled", "", "Error when checking" + ex.Message);
				printFeatureStatus(false);
				MessageBox.Show(ex.Message + Environment.NewLine
				                + ex.Source + Environment.NewLine
				                + ex.StackTrace + Environment.NewLine
				                + ex.TargetSite + Environment.NewLine
				                + ex.Data);
			}

		}

		private void CheckIIS6SubComponents()
		{
			using (var componentsKey =
				Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Setup\Oc Manager\Subcomponents", false)
				)
			{
				if (componentsKey != null)
				{
					printNewFeature("IIS", "IIS Management Console", "", "");
					printFeatureStatus(
						InternetInformationServicesDetection.IsInstalled(InternetInformationServicesComponent.InetMgr));
				}
			}

			// Case Sensitive
			const string webServerSchema = "IIsWebServer"; 
			const string serverName = "LocalHost";
			var w3Svc = new DirectoryEntry("IIS://" + serverName + "/w3svc/1/ROOT");
			foreach (DirectoryEntry site in w3Svc.Children)
			{
				if (site.SchemaClassName != webServerSchema) continue;

				printNewFeature("IIS", "ASP.NET Registered", "", "");
				try
				{
					printFeatureStatus(site.Properties["HttpCustomHeaders"].Value.ToString() == "X-Powered-By: ASP.NET");
				}
				catch
				{
					//We will assume that the feature is not installed.
					printFeatureStatus(false);
				}
				break;
			}

			using (var de = new DirectoryEntry("IIS://localhost/W3SVC"))
			{
				printNewFeature("IIS", "ASP.NET Enabled", "", "");
				foreach (string ext in de.Properties["WebSvcExtRestrictionList"])
				{
					if (ext.IndexOf("ASP.NET v2.0", StringComparison.Ordinal) == -1)
						continue;
					printFeatureStatus(ext.StartsWith("1,"));
				}
			}

		}
		#endregion

		private void ServiceCheck(string target, string serviceName)
		{
			//print something that people might recongise for SSIS
			printNewFeature(serviceName.Contains("MSDTSSERVER")
				                ? "SSIS Service"
				                : serviceName, "Service", "Running", "");

			listView1.Items[listView1.Items.Count - 1].SubItems[2].Text = CheckServiceRunning(target, serviceName);
			printFeatureStatus(CheckServiceRunning(target, serviceName) == "Running");
		}

		private void CheckDatabaseServices()
		{
			//Initialise as a default instance
			var dbServer = comboBoxSQLInstance.SelectedItem.ToString().Trim();
			var sqlEngineService = "MSSQLSERVER";
			var sqlAgentService = "SQLSERVERAGENT";
			var olapService = "MSSQLServerOLAPService";
			var ssis = "MSDTSSERVER";
			var sqlVersion = double.Parse(SQLServerFullVersion(ConnStringGet("master")).Substring(0, 4),
			                              CultureInfo.InvariantCulture);

			if (sqlVersion > 10)
				ssis = "MSDTSSERVER100";

			//Next, find out if this is a named instance
			var index = dbServer.IndexOf(@"\", StringComparison.Ordinal);
			var len = dbServer.Length;

			//If namned instance
			if (index != -1)
			{
				var dbInstance = dbServer.Substring(index + 1, len - index - 1);
				dbServer = dbServer.Substring(0, index);

				sqlEngineService = "MSSQL$" + dbInstance;
				sqlAgentService = "SQLAGENT$" + dbInstance;
				olapService = "MSOLAP$" + dbInstance;
			}

			//Check each needed service
			if (dbServer == "NoInstanceDetected")
				return;
			ServiceCheck(dbServer, sqlEngineService);
			ServiceCheck(dbServer, sqlAgentService);
			ServiceCheck(dbServer, olapService);
			ServiceCheck(dbServer, ssis);
		}

		private static string CheckServiceRunning(string server, string serviceName)
		{
			var serviceStatus = "";
			try
			{
				var scope = new ManagementScope("\\\\" + server + "\\root\\CIMV2");
				scope.Connect();
				var query = new ObjectQuery("SELECT * FROM Win32_Service WHERE Name = '" + serviceName + "'");
				var searcher = new ManagementObjectSearcher(scope, query);

				if (searcher.Get().Count == 0)
					serviceStatus = "Does not exist";

				foreach (ManagementObject queryObj in searcher.Get())
				{
					serviceStatus = queryObj["State"].ToString() == "Running"
						                ? "Running"
						                : queryObj["State"].ToString();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(string.Format("Wmi could not get service: {0}, With exception: {1}", serviceName, ex));
				serviceStatus = "WMI failed to get: " + serviceName;
			}
			return serviceStatus;
		}

		#region PrintStuff
		private void printNewFeature(string checkWhat, string feature, string minValue, string featureValue)
		{
			listView1.Items.Add(checkWhat);
			listView1.Items[listView1.Items.Count - 1].SubItems.Add(feature);
			listView1.Items[listView1.Items.Count - 1].SubItems.Add(featureValue);
			listView1.Items[listView1.Items.Count - 1].SubItems.Add(minValue);
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
		private void SQLInstanceEnum()
		{
			comboBoxSQLInstance.Items.Clear();

			var rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server");
			if (rk != null)
			{
				var instances = (String[]) rk.GetValue("InstalledInstances");
				if (instances != null)
				{
					if (instances.Length > 0)
					{
						foreach (var element in instances)
						{
							if (element == "MSSQLSERVER")
								comboBoxSQLInstance.Items.Add(Environment.MachineName);
							else
								comboBoxSQLInstance.Items.Add(Environment.MachineName + @"\" + element);
						}
					}
				}
			}
			else
				comboBoxSQLInstance.Items.Add("NoInstanceDetected");
		}

		//If shared web+Db server then get a higher number of agents
		private int AgentLevel()
		{
			var numberOfAgent = (int) numericUpDownAgents.Value;

			//Check if the server is intended to be both web AND db
			if (comboBoxServerSetup.SelectedItem.ToString().Contains("Web")
			    && comboBoxServerSetup.SelectedItem.ToString().Contains("DB"))
			{
				if (numberOfAgent < 500)
					numberOfAgent = numberOfAgent + 1500;
				else if (numberOfAgent <= 4000)
					numberOfAgent = numberOfAgent + 4000;
				else if (numberOfAgent > 4000)
					// ReSharper disable LocalizableElement
					MessageBox.Show("Warning! Running more then 4000 agents on a single server is not supported");
				// ReSharper restore LocalizableElement
			}
			return numberOfAgent;
		}

		//remove duplicates from a list of strings
		private static List<string> removeDuplicates(IEnumerable<string> input)
		{
			var tmp = new Dictionary<string, int>();

			foreach (var s in input)
				tmp[s] = 1;

			return new List<string>(tmp.Keys);
		}

		private static void CopyListViewToClipboard(ListView lv)
		{
			var buffer = new StringBuilder();

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
			var buffer = new StringBuilder();

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

		private void contextMenuStrip1_Click(object sender, EventArgs e)
		{
			if (comboBoxServerSetup.SelectedItem.ToString().Contains("Version6"))
				CopyAllListViewToClipboard();
			else
				CopyListViewToClipboard(listView1);
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
			var windowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent();
			if (windowsIdentity != null)
				SQLLogin.Text = windowsIdentity.Name;
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			comboBoxServerSetup.SelectedIndex = 0;
			groupBoxSQLInstance.Hide();
			var windowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent();
			if (windowsIdentity != null)
				SQLLogin.Text = windowsIdentity.Name;
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
		// ReSharper restore InconsistentNaming
	}
}