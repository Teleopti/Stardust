using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Forms;
using SevenZip;
using ProgressEventArgs = SevenZip.ProgressEventArgs;

namespace Teleopti.Support.Tool.Controls.DatabaseDeployment.Pages
{
	public partial class ProgressPage : SelectionPage
	{
		private readonly DatabaseDeploymentModel _model;

		public ProgressPage()
		{
			InitializeComponent();
		}

		public ProgressPage(DatabaseDeploymentModel model)
			: this()
		{
			_model = model;
		}


		private void RunProcess(ProcessStartInfo processStartInfo)
		{
			Process process = new Process();
			process.EnableRaisingEvents = true;
			process.OutputDataReceived += new DataReceivedEventHandler(ProcOnOutputDataReceived);
			process.ErrorDataReceived += new DataReceivedEventHandler(ProcOnErrorDataReceived);
			process.Exited += new EventHandler(ProcOnExited);
			process.StartInfo = processStartInfo;
			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
			while (!process.HasExited)
			{
				Application.DoEvents();
			}
			int exitCode = process.ExitCode;
			process.Dispose();

			if (exitCode != 0)
			{
				throw new ApplicationException(processStartInfo.FileName + " returned exit code: " + exitCode);
			}
		}

		private void AppendLine(string text)
		{
			if (InvokeRequired)
			{
				this.BeginInvoke(new Action<string>(AppendLine), new object[] { text });
				return;
			}


			textBoxOutput.Text += text;
			textBoxOutput.Text += Environment.NewLine;
			textBoxOutput.Select(textBoxOutput.Text.Length - 1, 0);
			textBoxOutput.ScrollToCaret();
			Application.DoEvents();
		}

		private void ProcOnExited(object sender, EventArgs eventArgs)
		{
			//buttonBack.Invoke(new EnableBackDelegate(EnableBack));
		}

		private void ProcOnErrorDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
		{
			AppendLine(dataReceivedEventArgs.Data);
		}

		private void ProcOnOutputDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
		{
			AppendLine(dataReceivedEventArgs.Data);
		}

		private void progressPage_Load(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			shareFolder(_model.UnzipPath, "DatabaseUnzipTemp", "Temporary folder for Teleopti support tool");
			copyFilesToShare();
			unzipFiles();
			restoreDatabase();
			createNhibFile();
			AppendLine("Updating cross references...");
			_model.Helper.UpdateCrossRef(_model.SelectedAggDatabase.DatabaseName, _model.SelectedAnalyticsDatabase.DatabaseName);
			recycleIisPools();

			Cursor = Cursors.Default;
			AppendLine("Database deployed");
		}

		private void recycleIisPools()
		{
			//TODO: DO IT!
		}

		private void createNhibFile()
		{
			throw new NotSupportedException("Cannot create nhib file. We don't use them any longer.");
		}

		private void unzipFiles()
		{
			if (_model.GetSelections().All(s => s.DatabaseFromSourceType != DatabaseSourceType.FromArchive) ||
				string.IsNullOrEmpty(_model.ZipFilePath)) return;

            SevenZipBase.SetLibraryPath(_model.SettingsInRegistry.LocationOf7zDll);
            SevenZipExtractor extractor = new SevenZipExtractor(_model.ZipFilePath);
            extractor.Extracting += extractor_Extracting;
            extractor.ExtractArchive(_model.UnzipPath);
		}

        void extractor_Extracting(object sender, ProgressEventArgs e)
        {
            if (((e.PercentDone / 10) * 10) == e.PercentDone)
            {
                AppendLine("Unzipping " + e.PercentDone + "%");
            }
        }

		private void copyFilesToShare()
		{
			var filesToCopy =
				_model.GetSelections().Where(s => s.DatabaseFromSourceType == DatabaseSourceType.ExistingFile)
					  .Select(s => s.DatabasePath)
					  .ToList();
			if (!filesToCopy.Any())
				return;
			if (!Directory.Exists(_model.UnzipPath))
				Directory.CreateDirectory(_model.UnzipPath);
			foreach (var file in filesToCopy)
			{
				var fileinfo = new FileInfo(file);
				AppendLine("Copying " + fileinfo.Name);
				File.Copy(file, _model.UnzipPath + fileinfo.Name, true);
			}
		}

		private void shareFolder(string folderPath, string shareName, string description)
		{
			folderPath = folderPath.TrimEnd('\\');
			// ReSharper disable LocalizableElement
			try
			{

				AppendLine("Creating shared folder...");
				var managementClass = new ManagementClass("Win32_Share");
				var inParams = managementClass.GetMethodParameters("Create");

				inParams["Description"] = description;
				inParams["Name"] = shareName;
				inParams["Path"] = folderPath;
				inParams["Type"] = 0x0; // Disk Drive
				//Another Type:
				//        DISK_DRIVE = 0x0
				//        PRINT_QUEUE = 0x1
				//        DEVICE = 0x2
				//        IPC = 0x3
				//        DISK_DRIVE_ADMIN = 0x80000000
				//        PRINT_QUEUE_ADMIN = 0x80000001
				//        DEVICE_ADMIN = 0x80000002
				//        IPC_ADMIN = 0x8000003
				inParams["MaximumAllowed"] = null;
				inParams["Password"] = null;
				inParams["Access"] = null; // Make Everyone has full control access.                
				//inParams["MaximumAllowed"] = int maxConnectionsNum;


				var outParams = managementClass.InvokeMethod("Create", inParams, null);
				if (outParams != null && (uint)(outParams.Properties["ReturnValue"].Value) != 0)
				{
					if ((uint)(outParams.Properties["ReturnValue"].Value) == 22)
						return;
					MessageBox.Show("Unable to share the folder: " + outParams.Properties["ReturnValue"].Value);
					//MessageBox.Show("Error sharing MMS folders. Please make sure you install as Administrator.", "Error!");
				}

                /*
                // Check to see if the method invocation was successful
                var result = (uint)(outParams.Properties["ReturnValue"].Value);
                switch (result)
                {
                    case 0:
                        Console.WriteLine("Folder successfuly shared.");
                        break;
                    case 2:
                        Console.WriteLine("Access Denied");
                        break;
                    case 8:
                        Console.WriteLine("Unknown Failure");
                        break;
                    case 9:
                        Console.WriteLine("Invalid Name");
                        break;
                    case 10:
                        Console.WriteLine("Invalid Level");
                        break;
                    case 21:
                        Console.WriteLine("Invalid Parameter");
                        break;
                    case 22:
                        Console.WriteLine("Duplicate Share");
                        break;
                    case 23:
                        Console.WriteLine("Redirected Path");
                        break;
                    case 24:
                        Console.WriteLine("Unknown Device or Directory");
                        break;
                    case 25:
                        Console.WriteLine("Net Name Not Found");
                        break;
                    default:
                        Console.WriteLine("Folder cannot be shared.");
                        break;
                }
                 */

				var ntAccount = new NTAccount("Everyone");
				var userSid = (SecurityIdentifier)ntAccount.Translate(typeof(SecurityIdentifier));
				var utenteSidArray = new byte[userSid.BinaryLength];
				userSid.GetBinaryForm(utenteSidArray, 0);

				//Trustee
				var userTrustee = new ManagementClass(new ManagementPath("Win32_Trustee"), null);
				userTrustee["Name"] = "Everyone";
				userTrustee["SID"] = utenteSidArray;

				//ACE
				var userAce = new ManagementClass(new ManagementPath("Win32_Ace"), null);
				userAce["AccessMask"] = 2032127;                                 //Full access
				userAce["AceFlags"] = AceFlags.ObjectInherit | AceFlags.ContainerInherit;
				userAce["AceType"] = AceType.AccessAllowed;
				userAce["Trustee"] = userTrustee;

				var userSecurityDescriptor = new ManagementClass(new ManagementPath("Win32_SecurityDescriptor"), null);
				userSecurityDescriptor["ControlFlags"] = 4; //SE_DACL_PRESENT 
				userSecurityDescriptor["DACL"] = new object[] { userAce };
				var share = new ManagementObject(managementClass.Path + ".Name='" + shareName + "'");

				share.InvokeMethod("SetShareInfo", new object[] { Int32.MaxValue, description, userSecurityDescriptor });
				AppendLine("Done creating shared folder.");

			}
			catch (Exception ex)
			{
				MessageBox.Show("Error sharing folders. Please make sure you install as Administrator. ERROR: " + ex.Message, "Error!");
			}
		}

		private void restoreDatabase()
		{
			var connectionString = _model.Helper.ConnectionString;
			var connection = new SqlConnection(connectionString);
			try
			{
				var selectedDatabases = _model.GetSelections();
				var databasesToRestore =
					selectedDatabases.Where(s => s.DatabaseFromSourceType != DatabaseSourceType.ExistingDatabase).ToList();

				if (!databasesToRestore.Any())
					return;

                var datafolder = _model.Helper.GetDataFolder();
                datafolder = datafolder + @"\" + _model.SessionName + @"\";
                if (!Directory.Exists(datafolder))
                {
                    Directory.CreateDirectory(datafolder);
                }

                connection.Open();
				foreach (var database in databasesToRestore)
				{
					SqlConnection.ClearPool(connection);
					var query = "";
					switch (database.DatabaseSource)
					{
						case BackupFileType.TeleoptiAnalytics:
							query = "IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + database.DatabaseName + "') " +
							        "DROP DATABASE " + database.DatabaseName +
							        string.Format(@" RESTORE DATABASE [{0}] FROM  DISK = N'{1}'" +
							                      @"WITH FILE = 1, " +
							                      @"MOVE N'TeleoptiAnalytics_Primary' TO N'{2}\{0}.mdf', " +
							                      @"MOVE N'TeleoptiAnalytics_Stage' TO N'{2}\{0}_1.ndf', " +
							                      @"MOVE N'TeleoptiAnalytics_Mart' TO N'{2}\{0}_2.ndf'," +
							                      @"MOVE N'TeleoptiAnalytics_Msg' TO N'{2}\{0}_3.ndf', " +
							                      @"MOVE N'TeleoptiAnalytics_Rta' TO N'{2}\{0}_4.ndf', " +
							                      @"MOVE N'TeleoptiAnalytics_Log' TO N'{2}\{0}_5.ldf', " +
							                      @"NOUNLOAD, " +
							                      @"STATS = 10 ", database.DatabaseName, _model.UnzipPath + database.DatabasePath, datafolder);
							break;
						case BackupFileType.TeleoptiCCC7:
							query = "IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + database.DatabaseName + "') " +
							        "DROP DATABASE " + database.DatabaseName +
							        string.Format(@" RESTORE DATABASE [{0}] FROM  DISK = N'{1}'" +
							                      @"WITH FILE = 1," +
							                      @"MOVE N'TeleoptiCCC7_Data' TO N'{2}\{0}.mdf', " +
							                      @"MOVE N'TeleoptiCCC7_Log' TO N'{2}\{0}_1.ndf', " +
							                      @"NOUNLOAD, " +
							                      @"STATS = 10 ", database.DatabaseName, _model.UnzipPath + database.DatabasePath, datafolder);
							break;
						case BackupFileType.TeleoptiCCCAgg:
							query = "IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + database.DatabaseName + "') " +
							        "DROP DATABASE " + database.DatabaseName +
							        string.Format(@" RESTORE DATABASE [{0}] FROM  DISK = N'{1}'" +
							                      @"WITH FILE = 1, " +
							                      @"MOVE N'TeleoptiCCCAgg_Data' TO N'{2}\{0}.mdf', " +
							                      @"MOVE N'TeleoptiCCCAgg_Log' TO N'{2}\{0}_1.ndf', " +
							                      @"NOUNLOAD, " +
							                      @"STATS = 10 ", database.DatabaseName, _model.UnzipPath + database.DatabasePath, datafolder);
							break;
					}
					SqlCommand command = connection.CreateCommand();
					command.CommandType = CommandType.Text;
					command.CommandText = query;
					command.CommandTimeout = 60*30;
                    command.Connection.InfoMessage += Connection_InfoMessage;
					AppendLine("Restoring " + database.DatabaseName);
                    //command.ExecuteNonQuery();

                    IAsyncResult asyncResult = command.BeginExecuteNonQuery();
                    while (!asyncResult.IsCompleted)
                    {
                        asyncResult.AsyncWaitHandle.WaitOne();
                        Application.DoEvents();
                    }
                    command.EndExecuteNonQuery(asyncResult);
				}
			}
			catch (Exception e)
			{
				AppendLine(e.Message);
			}
			connection.Close();
		}


        void Connection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            AppendLine(e.Message);
        }
	}
}
