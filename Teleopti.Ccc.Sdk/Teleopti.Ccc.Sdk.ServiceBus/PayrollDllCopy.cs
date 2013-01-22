using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;
using log4net;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public static class PayrollDllCopy
	{
		private static readonly Dictionary<string, DateTime> copiedFiles = new Dictionary<string, DateTime>();
		private static readonly ILog Log = LogManager.GetLogger(typeof(ServiceBusRunner));

		public static Dictionary<string, DateTime> CopiedFiles
		{
			get { return copiedFiles; }
		}

		public static void CopyPayrollDll()
		{
			try
			{
				var destination = new SearchPath().Path;
				var source = new SearchPath().PayrollDeployNewPath;
				CopyFiles(source, destination);
			}

			catch (Exception exception)
			{
				Log.Error("An exception was encountered when trying to load new payroll dll", exception);
				throw;
			}
		}

		public static void CopyPayrollDllTest(string source, string destination)
		{
			try
			{
				CopyFiles(source, destination);
			}
			catch (Exception exception)
			{
				Log.Error("An exception was encountered when trying to load new payroll dll", exception);
				throw;
			}
		}

		private static void CopyFiles(string source, string destination)
		{
			foreach (var folder in Directory.GetDirectories(source))
			{
				var newFolderPath = Path.GetFullPath(destination + Path.GetFileName(folder));

				if (!Directory.Exists(newFolderPath))
					Directory.CreateDirectory(newFolderPath);

				CopyFiles(folder, newFolderPath);
			}
			foreach (var file in Directory.GetFiles(source))
			{
				var totalSleepTime = 0;
				var fileInfo = new FileInfo(file);
				while (IsFileLocked(fileInfo))
				{
					Thread.Sleep(20);
					totalSleepTime += 20;
					if (totalSleepTime == 500) break;
				}

				if (totalSleepTime == 500)
				{
					Log.Warn(string.Format("File {0} have been in use for {1} milliseconds, skipping file", fileInfo.FullName,
										   totalSleepTime));
					continue;
				}

				if (file.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".settings", StringComparison.OrdinalIgnoreCase))
				{
					var xmlFileDestination = destination;
					while (!xmlFileDestination.EndsWith("\\Payroll", StringComparison.OrdinalIgnoreCase))
						xmlFileDestination = Directory.GetParent(xmlFileDestination).ToString();
					xmlFileDestination = Path.GetFullPath(xmlFileDestination + "\\" + Path.GetFileName(file));
					Log.Info(string.Format("Copying {0} to {1}", file, xmlFileDestination));
					File.Copy(file, xmlFileDestination, true);
				}

				else
				{
					var fileDestination = Path.GetFullPath(destination + "\\" + Path.GetFileName(file));
					Log.Info(string.Format("Copying {0} to {1}", file, fileDestination));
					File.Copy(file, fileDestination, true);
				}

				if (copiedFiles.ContainsKey(fileInfo.FullName))
					copiedFiles[fileInfo.FullName] = fileInfo.LastWriteTimeUtc;
				else
					copiedFiles.Add(fileInfo.FullName, fileInfo.LastWriteTimeUtc);
			}
		}

		private static bool IsFileLocked(FileInfo file)
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
