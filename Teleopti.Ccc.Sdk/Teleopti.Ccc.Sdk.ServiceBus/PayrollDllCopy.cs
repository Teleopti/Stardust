using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;
using log4net;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	[Serializable]
	public class PayrollDllCopy
	{
		public event EventHandler<FileSystemEventArgs> NewPayrollDll;
		private static readonly Dictionary<string, DateTime> CopiedFiles = new Dictionary<string, DateTime>();
		private static readonly ILog Log = LogManager.GetLogger(typeof(ServiceBusRunner));

		public void RunFileWatcher()
		{
			try
			{
				var fileWatchFolder = Path.GetFullPath(Environment.CurrentDirectory + "\\Payroll.DeployNew\\");
				if (!Directory.Exists(fileWatchFolder))
					Directory.CreateDirectory(fileWatchFolder);
				var watcher = new FileSystemWatcher(fileWatchFolder);
				watcher.NotifyFilter = NotifyFilters.LastWrite;
				watcher.Created += OnNewPayrollDll;
				watcher.Changed += OnNewPayrollDll;
				watcher.EnableRaisingEvents = true;
				watcher.IncludeSubdirectories = true;
			}
			catch (IOException exception)
			{
				Log.Error("An exception was encountered when configuring the custom payroll folder", exception);
				throw;
			}
		}

		public void OnNewPayrollDll(object sender, FileSystemEventArgs e)
		{
			var handler = NewPayrollDll;
			if (handler != null) handler(this, e);
		}

		public static void CopyPayrollDll()
		{
			try
			{
				var destination = new SearchPath().Path;
				var source = new DirectoryInfo(Path.GetFullPath(Environment.CurrentDirectory + "\\Payroll.DeployNew"));
				CopyFiles(source.ToString(), destination);
			}

			catch (Exception exception)
			{
				Log.Error("An exception was encountered when trying to load new payroll dll", exception);
			}
		}
		
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public static bool AreFilesUnlocked(object sender, FileSystemEventArgs e)
		{
			var totalSleepTime = 0;
			var info = new FileInfo(e.FullPath);

			if (!File.Exists(e.FullPath)
				|| (CopiedFiles.ContainsKey(e.FullPath)
				&& CopiedFiles[e.FullPath] == info.LastWriteTimeUtc
				&& CopiedFiles[e.FullPath].Millisecond == info.LastWriteTimeUtc.Millisecond))
			{
				Log.Info(string.Format("File {0} is already loaded", info.FullName));
				return false;
			}

			while (IsFileLocked(info))
			{
				const int sleepTime = 20;
				Thread.Sleep(sleepTime);
				totalSleepTime += sleepTime;
				if (totalSleepTime == 20)
					Log.Info(string.Format("File {0} is in use by another process", info.FullName));
				if (totalSleepTime == 500)
				{
					Log.Error(string.Format("File {0} is still in use after {1} seconds, aborting dll-change", info.FullName, totalSleepTime / 1000));
					return false;
				}
			}

			while (PayrollExportConsumer.IsRunning)
			{
				const int sleepTime = 5000;
				Thread.Sleep(sleepTime);
				totalSleepTime += sleepTime;
				if (totalSleepTime == 20)
					Log.Info("Payroll export is running");
				if (totalSleepTime == 7200000)
				{
					Log.Error(string.Format("Payroll export is still running after {0} hours, aborting dll-change", totalSleepTime / 3600000));
					return false;
				}
			}
			return true;
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

				if (file.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
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

				if (CopiedFiles.ContainsKey(fileInfo.FullName))
					CopiedFiles[fileInfo.FullName] = fileInfo.LastWriteTimeUtc;
				else
					CopiedFiles.Add(fileInfo.FullName, fileInfo.LastWriteTimeUtc);
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
