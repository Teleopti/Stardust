using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Support.Library;
using Teleopti.Support.Library.Config;
using Teleopti.Support.Library.Folders;
using Teleopti.Support.Security.Library;
using Parser = CommandLine.Parser;

namespace Teleopti.Develop.Batflow
{
	internal class Program
	{
		public static int Main(string[] args)
		{
			var log = new ConsoleLogger();
			var fixer = new DatabaseFixer(log);

			return Parser
				.Default
				.ParseArguments<FlowArguments, TestInitArguments>(args)
				.MapResult(
					(FlowArguments arguments) =>
					{
						var develop = new FixDatabasesCommand(arguments.DevelopBaseline, arguments.DevelopServer, null, DatabaseOperation.Ensure);
						fixer.Fix(develop);
						new FixMyConfigFixer().Fix(new FixMyConfigCommand
						{
							Server = arguments.DevelopServer,
							ApplicationDatabase = develop.ApplicationDatabase(),
							AnalyticsDatabase = develop.AnalyticsDatabase()
						});

						var test = new FixDatabasesCommand(arguments.TestBaseline, arguments.TestServer, "InfraTest", DatabaseOperation.Skip);
						fixer.Fix(test);
						new InfraTestConfigurator().Configure(new InfraTestConfigCommand
						{
							Server = arguments.TestServer,
							ApplicationDatabase = test.ApplicationDatabase(),
							AnalyticsDatabase = test.AnalyticsDatabase()
						});

						return 0;
					},
					(TestInitArguments arguments) =>
					{
						var test = new FixDatabasesCommand(arguments.Baseline, arguments.Server, "InfraTest", DatabaseOperation.Init);
						fixer.Fix(test);
						new InfraTestConfigurator().Configure(new InfraTestConfigCommand
						{
							Server = arguments.Server,
							ApplicationDatabase = test.ApplicationDatabase(),
							AnalyticsDatabase = test.AnalyticsDatabase()
						});

						return 0;
					},
					errs => 1);
		}

		[Verb("flow", HelpText = "Restores baseline for applications and configures test projects. Dont worry about it.")]
		public class FlowArguments
		{
			[Option("develop.baseline",
				Default = "DemoSales2017",
				HelpText = "Development database baseline to restore.")]
			public string DevelopBaseline { get; set; }

			[Option("develop.server",
				Default = ".",
				HelpText = "Development database server.")]
			public string DevelopServer { get; set; }

			[Option("test.baseline",
				Default = null,
				HelpText = "Test database baseline to restore. Builds databases if specified.")]
			public string TestBaseline { get; set; }

			[Option("test.server",
				Default = ".",
				HelpText = "Test database server.")]
			public string TestServer { get; set; }
		}

		[Verb("test.init", HelpText = "Initializes databases for test projects.")]
		public class TestInitArguments
		{
			[Option("server",
				Default = ".",
				HelpText = "Database server.")]
			public string Server { get; set; }

			[Option("baseline",
				Default = null,
				HelpText = "Database baseline to restore. Builds databases if specified.")]
			public string Baseline { get; set; }
		}

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
				string.IsNullOrEmpty(_databasePrefix) ? $"{RepositoryName()}_{_baseline}" : _databasePrefix;

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
				Console.WriteLine("Will try to fix databases like this:");
				consoleWrite();
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
				Console.WriteLine("Did that flow well?");
				consoleWrite();
			}

			private void consoleWrite()
			{
				Console.WriteLine($@"Operation: {Operation()}");
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

		public enum DatabaseOperation
		{
			Skip,
			Ensure,
			Init
		}

		public class DatabaseFixer
		{
			private readonly IUpgradeLog _log;

			public DatabaseFixer(IUpgradeLog log)
			{
				_log = log;
			}

			public void Fix(FixDatabasesCommand command)
			{
				if (command.Operation() == DatabaseOperation.Skip)
					return;

				command.ConsoleStart();

				patch(command, command.ApplicationDatabase(), command.ApplicationDatabaseBackup(), DatabaseType.TeleoptiCCC7);
				patch(command, command.AnalyticsDatabase(), command.AnalyticsDatabaseBackup(), DatabaseType.TeleoptiAnalytics);
				patch(command, command.AggDatabase(), command.AggDatabaseBackup(), DatabaseType.TeleoptiCCCAgg);

				new UpgradeRunner(_log).Upgrade(new UpgradeCommand
				{
					Server = command.Server(),
					ApplicationDatabase = command.ApplicationDatabase(),
					AnalyticsDatabase = command.AnalyticsDatabase(),
					AggDatabase = command.AggDatabase(),
					UseIntegratedSecurity = true
				});

				command.ConsoleEnd();
			}

			private void patch(FixDatabasesCommand command, string database, string backup, DatabaseType type)
			{
				var patchCommand = new PatchCommand
				{
					ServerName = command.Server(),
					DatabaseName = database,
					UseIntegratedSecurity = true,
					DatabaseType = type,
					UpgradeDatabase = true,
					DbManagerFolderPath = command.DatabaseSourcePath()
				};

				if (command.Operation() == DatabaseOperation.Ensure)
				{
					if (backup != null)
						patchCommand.RestoreBackupIfNotExistsOrNewer = backup;
					else
						patchCommand.RecreateDatabaseIfNotExistsOrNewer = true;
				}

				if (command.Operation() == DatabaseOperation.Init)
				{
					if (backup != null)
						patchCommand.RestoreBackup = backup;
					else
					{
						patchCommand.DropDatabase = true;
						patchCommand.CreateDatabase = true;
					}
				}

				new DatabasePatcher(_log).Run(patchCommand);
			}
		}
	}
}