using System;
using System.IO;
using System.Linq;
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
				.ParseArguments<FlowArguments, Placeholder>(args)
				.MapResult(
					(FlowArguments flowArguments) =>
					{
						var databases = new DatabaseFlowCommand(flowArguments.DevelopBaseline, flowArguments.DevelopServer);

						Console.WriteLine(@"Ok, I'll try with these:");
						Console.WriteLine($@"RepositoryPath: {databases.RepositoryPath()}");
						Console.WriteLine($@"RepositoryName: {databases.RepositoryName()}");
						Console.WriteLine($@"DatabasePath: {databases.DatabasePath()}");
						Console.WriteLine($@"DatabasePath: {databases.DatabasePath()}");
						Console.WriteLine($@"Server: {databases.Server()}");
						Console.WriteLine($@"ApplicationDatabase: {databases.ApplicationDatabase()}");
						Console.WriteLine($@"ApplicationDatabaseBackup: {databases.ApplicationDatabaseBackup()}");
						Console.WriteLine($@"AnalyticsDatabase: {databases.AnalyticsDatabase()}");
						Console.WriteLine($@"AnalyticsDatabaseBackup: {databases.AnalyticsDatabaseBackup()}");
						Console.WriteLine($@"AggDatabase: {databases.AggDatabase()}");
						Console.WriteLine($@"AggDatabaseBackup: {databases.AggDatabaseBackup()}");
						Console.WriteLine();

						new DatabaseFlower(log).Flow(databases);
						new FixMyConfigFixer().Fix(new FixMyConfigCommand
						{
							ApplicationDatabase = databases.ApplicationDatabase(),
							AnalyticsDatabase = databases.AnalyticsDatabase()
						});
						new InfraTestConfigurator().Configure(new InfraTestConfigCommand());

						Console.WriteLine();
						Console.WriteLine("Did that flow well?");

						return 0;
					},
					(Placeholder placeholder) =>
					{
						Console.WriteLine("Nothing to see here!");
						return 0;
					},
					errs => 1);
		}

		[Verb("lookhere", HelpText = "Very important feature")]
		public class Placeholder
		{
		}

		[Verb("flow", HelpText = "Set up things according to convention. Dont worry about it.")]
		public class FlowArguments
		{
			[Option("developserver", Default = ".", HelpText = "Development database server")]
			public string DevelopServer { get; set; }

			[Option("developbaseline", Default = "DemoSales2017", HelpText = "Development database baseline to restore")]
			public string DevelopBaseline { get; set; }

//			[Option("testserver", Default = ".", HelpText = "Test database server")]
//			public string TestServer { get; set; }
//
//			[Option("testbaseline", Default = "", HelpText = "Test database baseline to restore")]
//			public string TestBaseline { get; set; }
		}

		public class DatabaseFlowCommand
		{
			private readonly string _baseline;
			private readonly string _server;

			public DatabaseFlowCommand(string baseline, string server)
			{
				_baseline = baseline;
				_server = server;
			}

			public string Server() => _server;

			public string RepositoryPath() => new RepositoryRootFolder().Path();

			public string RepositoryName() =>
				new FileInfo(RepositoryPath()).Name
					.Replace(".", "")
					.Replace("'", "");

			public string DatabasePath() => Path.Combine(RepositoryPath(), "Database");
			public string SqlPassword() => "cadadi";
			public string SqlUserName() => "sa";

			public string ApplicationDatabaseBackup() => findFile($"{_baseline}_teleopticcc7.bak");
			public string AnalyticsDatabaseBackup() => findFile($"{_baseline}_TeleoptiAnalytics.bak");
			public string AggDatabaseBackup() => findFile($"{_baseline}_teleopticccagg.bak");

			public string ApplicationDatabase() => $"{RepositoryName()}_{_baseline}_TeleoptiWfm";
			public string AnalyticsDatabase() => $"{RepositoryName()}_{_baseline}_TeleoptiAnalytics";
			public string AggDatabase() => $"{RepositoryName()}_{_baseline}_TeleoptiAgg";

			private string findFile(string name)
			{
				var folders = new[] {".", ".com.teleopti.wfm.developer.tools"};

				var files = from d in folders
					let folderPath = Path.Combine(RepositoryPath(), d)
					where Directory.Exists(folderPath)
					let foundFiles = Directory.GetFiles(folderPath, name, SearchOption.TopDirectoryOnly)
					from f in foundFiles
					select f;

				return files.First();
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
					DbManagerFolderPath = command.DatabasePath(),
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
					DbManagerFolderPath = command.DatabasePath(),
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
					DbManagerFolderPath = command.DatabasePath(),
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