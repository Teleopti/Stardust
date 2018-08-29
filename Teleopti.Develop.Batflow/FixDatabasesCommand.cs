using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Teleopti.Support.Library.Folders;

namespace Teleopti.Develop.Batflow
{
	public class FixDatabasesCommand
	{
		private readonly string _baseline;
		private readonly string _server;
		private readonly string _databasePrefix;
		private readonly DatabaseOperation _databaseOperation;

		public FixDatabasesCommand(string baseline, string server, string databasePrefix, DatabaseOperation databaseOperation)
		{
			_baseline = baseline;
			_server = server;
			_databasePrefix = databasePrefix;
			_databaseOperation = databaseOperation;
		}

		public DatabaseOperation Operation() => _databaseOperation;
		public string Server() => _server;
		public string RepositoryPath() => new RepositoryRootFolder().Path();

		public string RepositoryName() =>
			new FileInfo(RepositoryPath()).Name
				.Replace(".", "")
				.Replace("'", "");

		public string DatabaseSourcePath() => Path.Combine(RepositoryPath(), "Database");

		public string ApplicationDatabaseBackup() => findBackupByHint("teleopticcc7", "ccc7", "app", "db");
		public string AnalyticsDatabaseBackup() => findBackupByHint("TeleoptiAnalytics", "analytics");
		public string AggDatabaseBackup() => findBackupByHint("TeleoptiAgg", "teleopticccagg", "agg");

		private string databasePrefix() =>
			String.IsNullOrEmpty(_databasePrefix) ? $"{RepositoryName()}_{_baseline}" : _databasePrefix;

		public string ApplicationDatabase() => $"{databasePrefix()}_TeleoptiWfm";
		public string AnalyticsDatabase() => $"{databasePrefix()}_TeleoptiAnalytics";
		public string AggDatabase() => $"{databasePrefix()}_TeleoptiAgg";

		private string findBackupByHint(params string[] hints)
		{
			var hintsUpper = hints.Select(x => x.ToUpper());
			var file = allBackups()
				.Where(x =>
				{
					var name = x.Name.ToUpper();
					if (_baseline != null)
						return hintsUpper.Any(hint => name == $"{_baseline}_{hint}.bak".ToUpper());
					return hintsUpper.Any(hint => name.Contains(hint));
				})
				.Select(x => x.FullName)
				.FirstOrDefault();
			return file;
		}

		private IEnumerable<FileInfo> allBackups()
		{
			var folders = new[] {"."};

			var files = from d in folders
				let folderPath = Path.Combine(RepositoryPath(), d)
				let directory = new DirectoryInfo(folderPath)
				where directory.Exists
				let foundFiles = directory.GetFiles("*.bak", SearchOption.TopDirectoryOnly)
				from f in foundFiles
				select f;

			return files;
		}

		public void ConsoleStart()
		{
			if (Operation() == DatabaseOperation.Ensure)
				Console.WriteLine(@"
Will ensure databases exists,
by creating or restoring if it doesnt exist, 
or recreating or restoring if existing is newer than the version you are at, 
or upgrade existing.
");
			if (Operation() == DatabaseOperation.Init)
				Console.WriteLine(@"
Will try to create or restore databases,
by creating or restoring if it doesnt exist, 
or recreating or restoring over existing.
");
			if (Operation() == DatabaseOperation.Drop)
				Console.WriteLine(@"
Will drop databases if they exist.
");
			consoleWrite();
			Console.WriteLine(@"Here we go...");
			Console.WriteLine();
		}

		public void ConsoleEnd()
		{
			Console.WriteLine($@"
              _,     _   _    ,_  
           o888P     Y8o8Y     Y888o.
         d88888      88888      88888b
       ,8888888b_  _d88888b_  _d8888888,
       888888888888888888888888888888888
       888888888888888888888888888888888
        Y8888P'Y888P'Y888P-Y888P'Y88888'
         Y888   '8'   Y8P   '8'   888Y
          '8o          V          o8'
             `                   `
");
			Console.WriteLine(@"Did that flow well?");
			consoleWrite();
		}

		private void consoleWrite()
		{
			Console.WriteLine($@"Repository: {RepositoryPath()}  ({RepositoryName()})");
			Console.WriteLine($@"Database source: {DatabaseSourcePath()}");
			Console.WriteLine($@"Server: {Server()}");
			Console.WriteLine($@"Databases:");
			Console.WriteLine($@"{ApplicationDatabase()}");
			Console.WriteLine($@"{AnalyticsDatabase()}");
			Console.WriteLine($@"{AggDatabase()}");
			Console.WriteLine($@"Backups:");
			Console.WriteLine($@"{ApplicationDatabaseBackup()}");
			Console.WriteLine($@"{AnalyticsDatabaseBackup()}");
			Console.WriteLine($@"{AggDatabaseBackup()}");
		}
	}
}