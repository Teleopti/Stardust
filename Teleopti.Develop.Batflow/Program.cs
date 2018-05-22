using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using CommandLine;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Support.Library;
using Teleopti.Support.Library.Config;
using Teleopti.Support.Library.Folders;
using Teleopti.Support.Security.Library;
using Teleopti.Support.Tool.Tool;
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
						var flower = new DatabaseFlower(log);

						var develop = new DatabaseFlowCommand(arguments.DevelopBaseline, arguments.DevelopServer, null);
						develop.ConsoleWrite();
						flower.Flow(develop);
						new FixMyConfigFixer().Fix(new FixMyConfigCommand
						{
							ApplicationDatabase = develop.ApplicationDatabase(),
							AnalyticsDatabase = develop.AnalyticsDatabase()
						});

						var test = new DatabaseFlowCommand(arguments.TestBaseline, arguments.TestServer, "InfraTest");
						test.ConsoleWrite();
						flower.Flow(test);
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
						var flower = new DatabaseFlower(log);

						var test = new DatabaseFlowCommand(arguments.Baseline, arguments.Server, "InfraTest");
						test.ConsoleWrite();
						flower.Flow(test);
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

		[Verb("test", HelpText = "Set up test projects according to convention.")]
		public class TestArguments
		{
			[Option("server", Default = ".", HelpText = "Database server")]
			public string Server { get; set; }

			[Option("baseline", Default = "", HelpText = "Database baseline to restore")]
			public string Baseline { get; set; }
		}

		[Verb("flow", HelpText = "Set up everything according to convention. Dont worry about it.")]
		public class FlowArguments
		{
			[Option("develop.server", Default = ".", HelpText = "Development database server")]
			public string DevelopServer { get; set; }

			[Option("develop.baseline", Default = "DemoSales2017", HelpText = "Development database baseline to restore")]
			public string DevelopBaseline { get; set; }

			[Option("test.server", Default = ".", HelpText = "Test database server")]
			public string TestServer { get; set; }

			[Option("test.baseline", Default = "", HelpText = "Test database baseline to restore")]
			public string TestBaseline { get; set; }
		}

		public class DatabaseFlowCommand
		{
			private readonly string _baseline;
			private readonly string _server;
			private readonly string _databasePrefix;

			public DatabaseFlowCommand(string baseline, string server, string databasePrefix)
			{
				_baseline = baseline;
				_server = server;
				_databasePrefix = databasePrefix;
			}

			public bool ShouldRestore() => !string.IsNullOrEmpty(_baseline);

			public string Server() => _server;

			public string RepositoryPath() => new RepositoryRootFolder().Path();

			public string RepositoryName() =>
				new FileInfo(RepositoryPath()).Name
					.Replace(".", "")
					.Replace("'", "");

			public string DatabaseSourcePath() => Path.Combine(RepositoryPath(), "Database");
			public string SqlPassword() => "cadadi";
			public string SqlUserName() => "sa";

			public string ApplicationDatabaseBackup() => findFile($"{_baseline}_teleopticcc7.bak");
			public string AnalyticsDatabaseBackup() => findFile($"{_baseline}_TeleoptiAnalytics.bak");
			public string AggDatabaseBackup() => findFile($"{_baseline}_teleopticccagg.bak");

			public string DatabasePrefix() =>
				string.IsNullOrEmpty(_databasePrefix) ? $"{RepositoryName()}_{_baseline}" : _databasePrefix;

			public string ApplicationDatabase() => $"{DatabasePrefix()}_TeleoptiWfm";
			public string AnalyticsDatabase() => $"{DatabasePrefix()}_TeleoptiAnalytics";
			public string AggDatabase() => $"{DatabasePrefix()}_TeleoptiAgg";

			private string findFile(string name)
			{
				var folders = new[] {".", ".com.teleopti.wfm.developer.tools"};

				var files = from d in folders
					let folderPath = Path.Combine(RepositoryPath(), d)
					where Directory.Exists(folderPath)
					let foundFiles = Directory.GetFiles(folderPath, name, SearchOption.TopDirectoryOnly)
					from f in foundFiles
					select f;

				return files.FirstOrDefault();
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

		public class DatabaseFlower
		{
			private readonly IUpgradeLog _log;


			public DatabaseFlower(IUpgradeLog log)
			{
				_log = log;
			}

			public void Flow(DatabaseFlowCommand command)
			{
				if (!command.ShouldRestore())
					return;

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