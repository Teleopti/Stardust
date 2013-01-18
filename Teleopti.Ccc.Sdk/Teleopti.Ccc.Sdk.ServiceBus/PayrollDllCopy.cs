using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;
using log4net;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public interface IPayrollDllCopy
	{
		event EventHandler NewPayrollFileExist;
		void RunFileWatcher();
		void CopyPayrollDll();
		void OnNewPayrollFileExist();
	}

	public class PayrollDllCopy : IPayrollDllCopy
	{
		public event EventHandler NewPayrollFileExist;
		
		private FileSystemWatcher _watcher;
		private static Dictionary<string, DateTime> _copiedFiles;
		private static readonly ILog Log = LogManager.GetLogger(typeof(ServiceBusRunner));

		public PayrollDllCopy()
		{
			_copiedFiles = new Dictionary<string, DateTime>();
		}
		
		public void RunFileWatcher()
		{
			try
			{
				var fileWatchFolder = Path.GetFullPath(Environment.CurrentDirectory + "\\Payroll.DeployNew\\");
				if (!Directory.Exists(fileWatchFolder))
					Directory.CreateDirectory(fileWatchFolder);
				_watcher = new FileSystemWatcher(fileWatchFolder);
				_watcher.NotifyFilter = NotifyFilters.LastWrite;
				_watcher.Created += OnChanged;
				_watcher.Changed += OnChanged;
				_watcher.EnableRaisingEvents = true;
				_watcher.IncludeSubdirectories = true;
			}
			catch (IOException exception)
			{
				Log.Error("An exception was encountered when configuring the custom payroll folder", exception);
				throw;
			}
		}
		
		public void CopyPayrollDll()
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

		public void OnNewPayrollFileExist()
		{
			var handler = NewPayrollFileExist;
			if (handler != null) handler(this, EventArgs.Empty);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private void OnChanged(object sender, FileSystemEventArgs e)
		{
			var totalSleepTime = 0;
			var info = new FileInfo(e.FullPath);

			if (!File.Exists(e.FullPath)
				|| (_copiedFiles.ContainsKey(e.FullPath)
				&& _copiedFiles[e.FullPath] == info.LastWriteTimeUtc
				&& _copiedFiles[e.FullPath].Millisecond == info.LastWriteTimeUtc.Millisecond))
				return;

			while (IsFileLocked(info))
			{
				const int sleepTime = 20;
				Thread.Sleep(sleepTime);
				totalSleepTime += sleepTime;
				if (totalSleepTime == 20)
					Log.Info(string.Format("File {0} is in use by another process", info.FullName));
				if (totalSleepTime == 500)
				{
					Log.Warn(string.Format("File {0} is still in use after {1} seconds, aborting dll-change", info.FullName, totalSleepTime / 1000));
					return;
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
					Log.Warn(string.Format("Payroll export is still running after {0} hours, aborting dll-change", totalSleepTime / 3600000));
					return;
				}
			}

			OnNewPayrollFileExist();
		}

		private void CopyFiles(string source, string destination)
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

				if (_copiedFiles.ContainsKey(fileInfo.FullName))
					_copiedFiles[fileInfo.FullName] = fileInfo.LastWriteTimeUtc;
				else
					_copiedFiles.Add(fileInfo.FullName, fileInfo.LastWriteTimeUtc);
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
