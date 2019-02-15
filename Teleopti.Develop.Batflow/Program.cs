using CommandLine;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Support.Library.Config;
using Teleopti.Wfm.Azure.Common;
using Parser = CommandLine.Parser;

namespace Teleopti.Develop.Batflow
{
	internal class Program
	{
		public static int Main(string[] args)
		{
			var env = new WfmInstallationEnvironment();
			var log = new ConsoleLogger();
			var fixer = new DatabaseFixer(log, env);

			return Parser
				.Default
				.ParseArguments<FlowVerb, TestInitVerb, TestDropVerb>(args)
				.MapResult(
					(FlowVerb verb) => flow(verb, fixer),
					(TestInitVerb verb) => testInit(verb, fixer),
					(TestDropVerb verb) => testDrop(verb, fixer),
					errs => 1);
		}

		private static int flow(FlowVerb verb, DatabaseFixer fixer)
		{
			var develop = new FixDatabasesCommand(verb.DevelopBaseline, verb.DevelopServer, null, DatabaseOperation.Ensure);
			fixer.Fix(develop);
			new FixMyConfigFixer().Fix(new FixMyConfigCommand
			{
				Server = verb.DevelopServer,
				ApplicationDatabase = develop.ApplicationDatabase(),
				AnalyticsDatabase = develop.AnalyticsDatabase()
			});

			var test = new FixDatabasesCommand(null, verb.TestServer, "InfraTest", DatabaseOperation.Skip);
			fixer.Fix(test);
			new InfraTestConfigurator().Configure(new InfraTestConfigCommand
			{
				Server = verb.TestServer,
				ApplicationDatabase = test.ApplicationDatabase(),
				AnalyticsDatabase = test.AnalyticsDatabase()
			});

			return 0;
		}

		private static int testInit(TestInitVerb verb, DatabaseFixer fixer)
		{
			var test = new FixDatabasesCommand(null, verb.Server, "InfraTest", DatabaseOperation.Init);
			fixer.Fix(test);
			new InfraTestConfigurator().Configure(new InfraTestConfigCommand
			{
				Server = verb.Server,
				ApplicationDatabase = test.ApplicationDatabase(),
				AnalyticsDatabase = test.AnalyticsDatabase(),
				ToggleMode = verb.ToggleMode
			});

			return 0;
		}

		private static int testDrop(TestDropVerb verb, DatabaseFixer fixer)
		{
			var test = new FixDatabasesCommand(null, verb.Server, "InfraTest", DatabaseOperation.Drop);
			fixer.Fix(test);
			return 0;
		}
	}

}