using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Support.Library.Config;

namespace Teleopti.Support.Tool.Tool
{
	public class CommandBuilder
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(CommandBuilder));

		private class Mode
		{
			public string Name;
			public object Command;
		}

		private IEnumerable<Mode> modes()
		{
			yield return new Mode {Name = "DEBUG", Command = new ModeDebugCommand()};
			yield return new Mode {Name = "DEPLOYWEB", Command = new ModeDeployWebCommand()};
			yield return new Mode {Name = "DEPLOYWORKER", Command = new ModeDeployWorkerCommand()};
			yield return new Mode {Name = "AZURE", Command = new ModeAzureCommand()};
			yield return new Mode {Name = "TEST", Command = new ModeTestCommand()};
		}

		public object ParseCommandLine(IEnumerable<string> args)
		{
			object command = null;

			logger.InfoFormat("support tool is called with args: {0}", string.Join(" ", args));

			args
				.ForEach(argument =>
				{
					matchSwitchWithAdjacentValue(argument, "-MO", v =>
					{
						var mode = modes().Single(x => x.Name == v.ToUpper());
						command = mode.Command;
					});
					matchSwitchWithAdjacentValue(argument, "-TC", v => { command = new SetToggleModeCommand(v); });
					matchSwitch(argument, new[] {"-?", "?", "-H", "-HELP", "HELP"}, () => { command = new HelpWindow(HelpWindow.HelpText); });
					matchSwitch(argument, "-BC", () =>
					{
						command = new ConfigurationBackupCommand
						{
							ConfigFiles = new ConfigFiles("DeployConfigFilesWeb.txt")
						};
					});
					matchSwitch(argument, "-RC", () => { command = new ConfigurationRestoreCommand(); });
					matchSwitch(argument, "-LOAD", () => { command = new LoadSettingsCommand {File = args.ElementAt(1)}; });
					matchSwitch(argument, "-SET", () => { command = new SetSettingCommand {SearchFor = args.ElementAt(1), ReplaceWith = args.ElementAt(2)}; });
					matchSwitch(argument, "-CS", () => { command = command = new ConfigServerCommand(args.ElementAt(1)); });
					matchSwitch(argument, "-PM", () => { command = new SavePmConfigurationCommand(args.ElementAt(1)); });
					matchSwitch(argument, "-FixMyConfig", () => { command = new FixMyConfigCommand {ApplicationDatabase = args.ElementAt(1), AnalyticsDatabase = args.ElementAt(2)}; });
					matchSwitch(argument, "-InfraTestConfig", () =>
					{
						var c = new InfraTestConfigCommand
						{
							ApplicationDatabase = args.ElementAt(1), 
							AnalyticsDatabase = args.ElementAt(2),
							ToggleMode = args.ElementAt(3)
						};
						if (args.Count() > 3)
							c.ToggleMode = args.ElementAt(3);
						if (args.Count() > 4)
							c.SqlAuthString = args.ElementAt(4);
						command = c;
					});
				});

			return command;
		}

		private static bool matchSwitch(string arg, string match, Action found) =>
			matchSwitch(arg, new[] {match}, found);

		private static bool matchSwitch(string arg, IEnumerable<string> matches, Action found)
		{
			var result = matches
				.Select(x => x.ToUpper())
				.Any(x => arg.ToUpper().Equals(x));
			if (result)
				found();
			return result;
		}

		private static bool matchSwitchWithAdjacentValue(string arg, string match, Action<string> value) =>
			matchSwitchWithAdjacentValue(arg, new[] {match}, value);

		private static bool matchSwitchWithAdjacentValue(string arg, IEnumerable<string> matches, Action<string> value)
		{
			var match = matches
				.Select(x => x.ToUpper())
				.OrderByDescending(x => x.Length)
				.FirstOrDefault(arg.StartsWith);

			if (match == null)
				return false;

			var v = arg.Remove(0, match.Length);
			value(v);
			return true;
		}
	}
}