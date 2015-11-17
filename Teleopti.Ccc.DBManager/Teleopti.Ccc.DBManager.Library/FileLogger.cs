using System;
using System.Globalization;
using System.IO;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DBManager.Library
{
	public class FileLogger : IUpgradeLog
	{
		private readonly TextWriter _logFile;

		public FileLogger()
		{
			var nowDateTime = DateTime.Now;
			_logFile = new StreamWriter(string.Format(CultureInfo.CurrentCulture, "DBManagerLibrary_{0}_{1}.log", nowDateTime.ToString("yyyyMMdd", CultureInfo.CurrentCulture), nowDateTime.ToString("hhmmss", CultureInfo.CurrentCulture)));
		}

		public void Write(string message)
		{
			Console.Out.WriteLine(message);
			_logFile.WriteLine(message);
		}

		public void Write(string message, string level)
		{
			Write(message);
		}

		public void Dispose()
		{
			if (_logFile == null)
				return;
			_logFile.Close();
			_logFile.Dispose();
		}
	}
}