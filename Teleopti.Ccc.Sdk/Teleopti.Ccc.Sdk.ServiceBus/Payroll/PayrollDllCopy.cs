using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using log4net;
using Microsoft.WindowsAzure.Storage;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
	public class PayrollDllCopy
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(PayrollDllCopy));
		private readonly ISearchPath _searchPath;
		private readonly IConfigReader _configReader;

		public PayrollDllCopy(ISearchPath searchPath, IConfigReader configReader = null)
		{
			_searchPath = searchPath;
			_configReader = configReader ?? new ConfigReader();
		}

		public void CopyPayrollDll()
		{
			try
			{
				var destination = _searchPath.Path;
				var source = _searchPath.PayrollDeployNewPath;
				copyFiles(source, destination);
			}

			catch (Exception exception)
			{
				Log.Error("An exception was encountered when trying to load new payroll dll", exception);
				throw;
			}
		}

		public List<string> CopyPayrollDllFromAzureStorage(string tenantName)
		{
			try
			{
				return CopyFilsesFromAzureStorage(tenantName, _configReader.ConnectionString("AzureStorage"),
					_configReader.AppConfig("AzureStorageContainer"), _configReader.AppConfig("AzureStoragePayrollPath"),
					_searchPath.Path);
			}
			catch (Exception exception)
			{
				Log.Error("An exception was encountered when trying to load new payroll dll from azure storage.", exception);
				throw;
			}
		}

		public static List<string> CopyFilsesFromAzureStorage(string tenantName, string connectionString, string containerName, string payrollPath, string destination)
		{
			var addedFiles = new List<string>();
			var account = CloudStorageAccount.Parse(connectionString);

			//Get storage account reference
			var storageAccount = new CloudStorageAccount(account.Credentials, true);

			// Create a blob client for interacting with the blob service.
			var blobClient = storageAccount.CreateCloudBlobClient();

			//Getting container reference
			var container = blobClient.GetContainerReference(containerName);

			var listOfUrisToDownload = container.ListBlobs($"{payrollPath}/{tenantName}/", true).Select(x => x.Uri).ToList();

			//Ensure path exist and add tenantName to path.
			destination += $"{tenantName}/";
			Directory.CreateDirectory(destination);


			foreach (var uri in listOfUrisToDownload)
			{
				var fileName = uri.Segments[uri.Segments.Length - 1];
				addedFiles.Add(fileName);

				var fileNameAndDestination = $"{destination}{fileName}";
				if(File.Exists(fileNameAndDestination))
				{
					File.Delete(fileNameAndDestination);
				}

				container.GetBlockBlobReference(uri.LocalPath.Replace($"/{containerName}/", "")).DownloadToFile($"{fileNameAndDestination}", FileMode.CreateNew);
			}

			return addedFiles;
		}
		
		public static void CopyFiles(string sourcePath, string destinationPath,
			string subdirectoryPath)
		{
			var fullSourcePath = Path.Combine(sourcePath, subdirectoryPath);
			if(Directory.Exists(fullSourcePath) == false)
				return;
			var fullDestinationPath = Path.Combine(destinationPath, subdirectoryPath);

			copyFiles(fullSourcePath, fullDestinationPath);
		}

		private static void copyFiles(string source, string destination)
		{

			if (!Directory.Exists(destination))
				Directory.CreateDirectory(destination);

			foreach (var folder in Directory.GetDirectories(source))
			{
				var newFolderPath = Path.GetFullPath(destination + Path.GetFileName(folder));

				if (!Directory.Exists(newFolderPath))
					Directory.CreateDirectory(newFolderPath);

				copyFiles(folder, newFolderPath);
			}
			foreach (var file in Directory.GetFiles(source))
			{
				var totalSleepTime = 0;
				var fileInfo = new FileInfo(file);
				while (isFileLocked(fileInfo))
				{
					Thread.Sleep(20);
					totalSleepTime += 20;
					if (totalSleepTime == 500) break;
				}

				if (totalSleepTime == 500)
				{
					Log.Warn($"File {fileInfo.FullName} have been in use for {totalSleepTime} milliseconds, skipping file");
					continue;
				}

				if (file.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".settings", StringComparison.OrdinalIgnoreCase))
				{
					var xmlFileDestination = destination;
					while (!xmlFileDestination.EndsWith("\\Payroll", StringComparison.OrdinalIgnoreCase))
						xmlFileDestination = Directory.GetParent(xmlFileDestination).ToString();
					xmlFileDestination = Path.GetFullPath(xmlFileDestination + "\\" + Path.GetFileName(file));
					Log.Info($"Copying {file} to {xmlFileDestination}");
					File.Copy(file, xmlFileDestination, true);
				}

				else
				{
					var fileDestination = Path.GetFullPath(destination + "\\" + Path.GetFileName(file));
					Log.Info($"Copying {file} to {fileDestination}");
					File.Copy(file, fileDestination, true);
				}
			}
		}

		private static bool isFileLocked(FileInfo file)
		{
			FileStream stream = null;
			try
			{
				stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
			}
			catch (IOException)
			{
				return true;
			}
			finally
			{
				if (stream != null)
					stream.Close();
			}
			return false;
		}
	}
}
