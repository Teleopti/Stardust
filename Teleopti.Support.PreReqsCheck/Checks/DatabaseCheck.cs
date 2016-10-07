﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CheckPreRequisites.Checks
{
	public class DatabaseCheck
	{
		// ReSharper disable InconsistentNaming
		private readonly Form1 _form1;

		public DatabaseCheck(Form1 owner)
		{
			_form1 = owner;
		}

		public void DBConnectionCheck(ref bool dbConn, string connString, string dbName)
		{
			var lineNumber = _form1.printNewFeature("Database", "DB Connection", "", "Connecting: " + dbName);

			using (var conn = new SqlConnection(connString))
			{
				try
				{
					conn.Open();
					if (conn.State == ConnectionState.Open)
						_form1.printFeatureStatus(true, "", lineNumber);
					conn.Close();
					dbConn = true;
				}
				catch
				{
					_form1.printFeatureStatus(false, "", lineNumber);
				}
			}
		}

		public void RunDbChecks(string dbServer)
		{
			var sqlVersion = CheckDatabaseVersion();
			CheckDatabaseServicesAndComponents(dbServer,sqlVersion);
			DBSoftwareCheck(sqlVersion);
		}

		private double CheckDatabaseVersion()
		{
			var sqlVersion = Double.Parse(SQLServerFullVersion(_form1.ConnStringGet("master")).Substring(0, 4),
										  CultureInfo.InvariantCulture);
			return sqlVersion;
		}

		private void CheckDatabaseServicesAndComponents(string dbServer, double sqlVersion)
		{
			var sqlEngineService = "MSSQLSERVER";
			var sqlAgentService = "SQLSERVERAGENT";
			var olapService = "MSSQLServerOLAPService";
			//var ssis = "MSDTSSERVER100";
			//if (sqlVersion >= 11)
			//	ssis = "MSDTSSERVER110";

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
			//ServiceCheck(dbServer, ssis);
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

		private static double SQLServerMajorVersion(string connString)
		{
			double result;
			const string versionCommand = "select convert(char(20), serverproperty('ProductVersion'))";
			using (var conn = new SqlConnection(connString))
			{
				conn.Open();
				using (var cmd = new SqlCommand(versionCommand, conn))
				using (var reader = cmd.ExecuteReader())
				{
					reader.Read();
					var res = reader.GetString(0).Split(new[] {'.'});
					result = double.Parse(res[0]);
					result = result + (double.Parse(res[1]) / 100);
				}
				conn.Close();
			}
			return result;
		}

		private void ServiceCheck(string target, string serviceName)
		{
			//print something that people might recongise for SSIS
			var lineNumber= _form1.printNewFeature(serviceName.Contains("MSDTSSERVER")
				                       ? "SSIS Service"
				                       : serviceName, "Service", "Running", "");

			_form1.listView1.Items[_form1.listView1.Items.Count - 1].SubItems[2].Text = CheckServiceRunning(target, serviceName);
			_form1.printFeatureStatus(CheckServiceRunning(target, serviceName) == "Running", "", lineNumber);
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
				MessageBox.Show(String.Format("Wmi could not get service: {0}, With exception: {1}", serviceName, ex));
				serviceStatus = "WMI failed to get: " + serviceName;
			}
			return serviceStatus;
		}

		public void DBSoftwareCheck(double sqlVersion)
		{
			//SQL Management Studio - Used for sql management and development
			var software = "Microsoft SQL Server Management Studio";
			var minValue = "SQL Management Studio 2008/2012";
			CheckSoftware(software, minValue);

			if (sqlVersion >= 11)
			{
				//SQL Server Data Tools - New tool used for SSIS development
				software = "Microsoft SQL Server Data Tools";
				minValue = "Microsoft SQL Server Data Tools";
			}
			else
			{
				//SQL BI Studio - Used for SSIS development
				software = "Microsoft SQL Server Development Studio";
				minValue = "SQL BI Studio 2008/2012";
			}
			CheckSoftware(software, minValue);
		}

		private void CheckSoftware(string software, string minValue)
		{
			var words = software.Split(' ');
			var softwareList = ListSoftware(words);

			if (softwareList.Count > 0)
			{
				foreach (string s in softwareList)
				{
					var lineNumber = _form1.printNewFeature("Database", "Software", minValue, s);
					_form1.printFeatureStatus(true, "", lineNumber);
				}
			}
			else
			{
				var lineNumber = _form1.printNewFeature("Database", "Software", minValue, software);
				_form1.printFeatureStatus("Check Manually", lineNumber);
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
							catch
							{
							}
						}
					}
			}
			//duplicates RegKeys seems to reported for many products. Remove them before return
			foundSoftware = removeDuplicates(foundSoftware);

			return foundSoftware;
		}

		private static List<string> removeDuplicates(IEnumerable<string> input)
		{
			var tmp = new Dictionary<string, int>();

			foreach (var s in input)
				tmp[s] = 1;

			return new List<string>(tmp.Keys);
		}

		public void CheckDbInternals(string dbName)
		{
			var connectionString = _form1.ConnStringGet("master");
			var dbConnection = false;

			DBConnectionCheck(ref dbConnection, connectionString, dbName);

			if (!dbConnection) 
				return;

			DBVersionCheck(connectionString);
			DBSysAdminCheck(connectionString);
			DBCollationCheck(connectionString);
			WriteTestTempDb(connectionString);
		}

		private void DBVersionCheck(string connString)
		{
			var dbMajorVersion = SQLServerMajorVersion(connString);
			var dbFullVersion = SQLServerFullVersion(connString);

			var lineNumber = _form1.printNewFeature("Database", "Full Version", "MS SQL Server 2008 R2 or later", dbFullVersion);
			_form1.printFeatureStatus(dbMajorVersion >= 10.5, "", lineNumber);
		}

		private void DBSysAdminCheck(string connString)
		{
			const string sysAdminCommand = "SELECT IS_SRVROLEMEMBER('sysadmin')";
			var lineNumber = _form1.printNewFeature("Database", "Permission", "SysAdmin role required", "Sys Admin role");

			using (var conn = new SqlConnection(connString))
			{
				try
				{
					conn.Open();
					using (var cmd = new SqlCommand(sysAdminCommand, conn))
					using (var reader = cmd.ExecuteReader())
					{
						reader.Read();
						_form1.printFeatureStatus(reader.GetInt32(0) == 1, "", lineNumber);
					}
					conn.Close();
				}
				catch
				{
					_form1.printFeatureStatus(false, "", lineNumber);
				}
			}
		}

		private void DBCollationCheck(string connString)
		{
			var collation = SQLServerCollation(connString);
			var lineNumber = _form1.printNewFeature("Database", "Collation", "SQL_Latin1_General_CP1_CI_AS", collation);

			if (collation.ToUpper().Contains("_BIN")) //Binary collation
				_form1.printFeatureStatus(false, "", lineNumber);

			else if (collation.ToUpper().Contains("_CS_")) //Case Sensitive
				_form1.printFeatureStatus("Warning", lineNumber);
			else
				_form1.printFeatureStatus(true, "", lineNumber);
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

		private void WriteTestTempDb(string connString)
		{
			var regex = new Regex("^\\s*GO\\s*$",
			                      RegexOptions.IgnoreCase |
			                      RegexOptions.Multiline);
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

							var serverLanSpeedLineNumber = _form1.printNewFeature("Database", "Server Lan speed [mbps]", "200",
							                       bandWidth.ToString(CultureInfo.InvariantCulture));
							_form1.printFeatureStatus(true, "", serverLanSpeedLineNumber);


							//IOLatency figures
							reader.NextResult();
							reader.Read();

							var ioWriteLatency = reader.GetInt64(0);
							var displayIoWriteLatency = "<1";

							if (ioWriteLatency > 0)
								displayIoWriteLatency = ioWriteLatency.ToString(CultureInfo.InvariantCulture);

							var ioWriteLatencyLineNumber = _form1.printNewFeature("Database", "IoWriteLatency [ms]", "5", displayIoWriteLatency);
							_form1.printFeatureStatus(ioWriteLatency <= 5, "", ioWriteLatencyLineNumber);

							reader.Close();
						}
					}
					finally
					{
						file?.Close();
					}
				}
				conn.Close();
			}
		}

		// ReSharper restore InconsistentNaming
	}
}
