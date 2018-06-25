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
			return Parser
				.Default
				.ParseArguments<FlowArguments, TestArguments>(args)
				.MapResult(
					(FlowArguments arguments) =>
					{
						var databaseBuilder = new DatabaseBuilder(log);

						var develop = new BuildDatabasesCommand(arguments.DevelopBaseline, arguments.DevelopServer, null);
						develop.ConsoleWrite();
						if (buildDatabases(arguments.DevelopBaseline, arguments.DevelopBuildDatabases))
							databaseBuilder.Build(develop);
						new FixMyConfigFixer().Fix(new FixMyConfigCommand
						{
							ApplicationDatabase = develop.ApplicationDatabase(),
							AnalyticsDatabase = develop.AnalyticsDatabase()
						});

						var test = new BuildDatabasesCommand(arguments.TestBaseline, arguments.TestServer, "InfraTest");
						test.ConsoleWrite();
						if (buildDatabases(arguments.TestBaseline, arguments.TestBuildDatabases))
							databaseBuilder.Build(test);
						new InfraTestConfigurator().Configure(new InfraTestConfigCommand
						{
							ApplicationDatabase = test.ApplicationDatabase(),
							AnalyticsDatabase = test.AnalyticsDatabase()
						});

						Console.WriteLine();
						Console.WriteLine("Did that flow well?");

						return 0;
					},
					(TestArguments arguments) =>
					{
						var databaseBuilder = new DatabaseBuilder(log);

						var test = new BuildDatabasesCommand(arguments.Baseline, arguments.Server, "InfraTest");
						test.ConsoleWrite();
						if (buildDatabases(arguments.Baseline, arguments.BuildDatabases))
							databaseBuilder.Build(test);
						new InfraTestConfigurator().Configure(new InfraTestConfigCommand
						{
							ApplicationDatabase = test.ApplicationDatabase(),
							AnalyticsDatabase = test.AnalyticsDatabase()
						});

						Console.WriteLine();
						Console.WriteLine("Did that flow well?");

						return 0;
					},
					errs => 1);
		}

		public static bool buildDatabases(string baseline, bool buildDatabases) =>
			!string.IsNullOrEmpty(baseline) || buildDatabases;

		[Verb("test", HelpText = "Set up test projects according to convention.")]
		public class TestArguments
		{
			[Option("buildDatabases", Default = false, HelpText = "Build databases.")]
			public bool BuildDatabases { get; set; }

			[Option("server", Default = ".", HelpText = "Database server.")]
			public string Server { get; set; }

			[Option("baseline", Default = null, HelpText = "Database baseline to restore. Builds databases if specified.")]
			public string Baseline { get; set; }
		}

		[Verb("flow", HelpText = "Set up everything according to convention. Dont worry about it.")]
		public class FlowArguments
		{
			[Option("develop.buildDatabases", Default = false, HelpText = "Build development databases.")]
			public bool DevelopBuildDatabases { get; set; }

			[Option("develop.server", Default = ".", HelpText = "Development database server.")]
			public string DevelopServer { get; set; }

			[Option("develop.baseline", Default = "DemoSales2017",
				HelpText = "Development database baseline to restore. Builds databases if specified.")]
			public string DevelopBaseline { get; set; }

			[Option("test.buildDatabases", Default = false, HelpText = "Build test databases.")]
			public bool TestBuildDatabases { get; set; }

			[Option("test.server", Default = ".", HelpText = "Test database server.")]
			public string TestServer { get; set; }

			[Option("test.baseline", Default = null,
				HelpText = "Test database baseline to restore. Builds databases if specified.")]
			public string TestBaseline { get; set; }
		}

		public class BuildDatabasesCommand
		{
			private readonly string _baseline;
			private readonly string _server;
			private readonly string _databasePrefix;

			public BuildDatabasesCommand(string baseline, string server, string databasePrefix)
			{
				_baseline = baseline;
				_server = server;
				_databasePrefix = databasePrefix;
			}

			public string Server() => _server;

			public string RepositoryPath() => new RepositoryRootFolder().Path();

			public string RepositoryName() =>
				new FileInfo(RepositoryPath()).Name
					.Replace(".", "")
					.Replace("'", "");

			public string DatabaseSourcePath() => Path.Combine(RepositoryPath(), "Database");
			public string SqlPassword() => "cadadi";
			public string SqlUserName() => "sa";

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
				var folders = new[] {".", ".com.teleopti.wfm.developer.tools"};

				var files = from d in folders
					let folderPath = Path.Combine(RepositoryPath(), d)
					let directory = new DirectoryInfo(folderPath)
					where directory.Exists
					let foundFiles = directory.GetFiles("*.bak", SearchOption.TopDirectoryOnly)
					from f in foundFiles
					select f;

				return files;
			}

			public void ConsoleWrite()
			{
				Console.WriteLine();
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
				Console.WriteLine();
			}
		}

		public class DatabaseBuilder
		{
			private readonly IUpgradeLog _log;

			public DatabaseBuilder(IUpgradeLog log)
			{
				_log = log;
			}

			public void Build(BuildDatabasesCommand command)
			{
				new DatabasePatcher(_log).Run(new PatchCommand
				{
					ServerName = command.Server(),
					DatabaseName = command.ApplicationDatabase(),
					UseIntegratedSecurity = true,
					DatabaseType = DatabaseType.TeleoptiCCC7,
					UpgradeDatabase = true,
					CreatePermissions = true,
					AppUserName = command.SqlUserName(),
					AppUserPassword = command.SqlPassword(),
					DbManagerFolderPath = command.DatabaseSourcePath(),
					CreateDatabase = command.ApplicationDatabaseBackup() == null,
					RestoreBackupIfNotExistsOrNewer = command.ApplicationDatabaseBackup()
				});

				new DatabasePatcher(_log).Run(new PatchCommand
				{
					ServerName = command.Server(),
					DatabaseName = command.AnalyticsDatabase(),
					UseIntegratedSecurity = true,
					DatabaseType = DatabaseType.TeleoptiAnalytics,
					UpgradeDatabase = true,
					CreatePermissions = true,
					AppUserName = command.SqlUserName(),
					AppUserPassword = command.SqlPassword(),
					DbManagerFolderPath = command.DatabaseSourcePath(),
					CreateDatabase = command.AnalyticsDatabaseBackup() == null,
					RestoreBackupIfNotExistsOrNewer = command.AnalyticsDatabaseBackup()
				});

				new DatabasePatcher(_log).Run(new PatchCommand
				{
					ServerName = command.Server(),
					DatabaseName = command.AggDatabase(),
					UseIntegratedSecurity = true,
					DatabaseType = DatabaseType.TeleoptiCCCAgg,
					UpgradeDatabase = true,
					CreatePermissions = true,
					AppUserName = command.SqlUserName(),
					AppUserPassword = command.SqlPassword(),
					DbManagerFolderPath = command.DatabaseSourcePath(),
					CreateDatabase = command.AggDatabaseBackup() == null,
					RestoreBackupIfNotExistsOrNewer = command.AggDatabaseBackup()
				});

				new UpgradeRunner(_log).Upgrade(new UpgradeCommand
				{
					Server = command.Server(),
					ApplicationDatabase = command.ApplicationDatabase(),
					AnalyticsDatabase = command.AnalyticsDatabase(),
					AggDatabase = command.AggDatabase(),
					UseIntegratedSecurity = true
				});
			}
		}
	}
}