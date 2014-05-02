using System;
using System.IO;
using System.Threading;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;
using log4net;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public static class PayrollDllCopy
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(ServiceBusRunner));
		
		public static void CopyPayrollDll()
		{
			try
			{
				var destination = new SearchPath().Path;
				var source = new SearchPath().PayrollDeployNewPath;
				copyFiles(source, destination);
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
				copyFiles(source, destination);
			}
			catch (Exception exception)
			{
				Log.Error("An exception was encountered when trying to load new payroll dll", exception);
				throw;
			}
		}

		private static void copyFiles(string source, string destination)
		{
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
