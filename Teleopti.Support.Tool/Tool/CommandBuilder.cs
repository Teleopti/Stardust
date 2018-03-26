using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Support.Library.Config;

namespace Teleopti.Support.Tool.Tool
{
	public class CommandBuilder
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(CommandBuilder));

		private ConfigFiles configFiles;
		private CompositeCommand modeDefaultCommand;
		private CompositeCommand modeDeployCommand;
		private ConfigurationBackupCommand backupCommand;
		private ISupportCommand restoreCommand;

		public CommandBuilder()
		{
			var configFilePathReader = new ConfigFilePathReader();
			var customSection = new CustomSection();
			var restoreCommand = new ConfigurationRestoreCommand(customSection, configFilePathReader, () => configFiles);
			backupCommand = new ConfigurationBackupCommand(customSection, configFilePathReader, () => configFiles);
			var refreshConfigFile = new RefreshConfigFile();
			var refreshConfigsRunner = new RefreshConfigsRunner(refreshConfigFile, () => configFiles);

			modeDefaultCommand = new CompositeCommand(
				refreshConfigsRunner,
				restoreCommand,
				new MoveCustomReportsCommand()
			);

			modeDeployCommand = new CompositeCommand(
				refreshConfigsRunner,
				restoreCommand
			);
		}

		private class Mode
		{
			public string Name;
			public ConfigFiles ConfigFiles;
			public ISupportCommand Command;
		}

		private IEnumerable<Mode> modes()
		{
			yield return new Mode {Name = "DEBUG", ConfigFiles = new ConfigFiles("ConfigFiles.txt"), Command = modeDefaultCommand};
			yield return new Mode {Name = "DEPLOYWEB", ConfigFiles = new ConfigFiles("DeployConfigFilesWeb.txt"), Command = modeDeployCommand};
			yield return new Mode {Name = "DEPLOYWORKER", ConfigFiles = new ConfigFiles("DeployConfigFilesWorker.txt"), Command = modeDeployCommand};
			yield return new Mode {Name = "AZURE", ConfigFiles = new ConfigFiles("AzureConfigFiles.txt"), Command = modeDefaultCommand};
			yield return new Mode {Name = "TEST", ConfigFiles = new ConfigFiles("BuildServerConfigFiles.txt"), Command = modeDefaultCommand};
		}

		public ISupportCommand ParseCommandLine(IEnumerable<string> args)
		{
			ISupportCommand command = null;
			configFiles = null;

			logger.InfoFormat("support tool is called with args: {0}", string.Join(" ", args));

			args
				.Select(a => a.ToUpper())
				.ForEach(argument =>
				{
					matchSwitchWithAdjacentValue(argument, "-MO", v =>
					{
						var mode = modes().Single(x => x.Name == v);
						configFiles = mode.ConfigFiles;
						command = mode.Command;
					});
					matchSwitchWithAdjacentValue(argument, "-TC", v => { command = new SetToggleModeCommand(v); });
					matchSwitch(argument, new[] {"-?", "?", "-H", "-HELP", "HELP"}, () => { command = new HelpWindow(HelpWindow.HelpText); });
					matchSwitch(argument, "-BC", () =>
					{
						configFiles = new ConfigFiles("DeployConfigFilesWeb.txt");
						command = backupCommand;
					});
					matchSwitch(argument, "-RC", () => { command = restoreCommand; });
					matchSwitch(argument, "-SET", () => { command = new SetSettingCommand(args.ElementAt(1), args.ElementAt(2)); });
					matchSwitch(argument, "-CS", () => { command = command = new ConfigServerCommand(args.ElementAt(1)); });
					matchSwitch(argument, "-PM", () => { command = new SavePmConfigurationCommand(args.ElementAt(1)); });
				});

			return command;
		}

		private static bool matchSwitch(string arg, string match, Action found) =>
			matchSwitch(arg, new[] {match}, found);

		private static bool matchSwitch(string arg, IEnumerable<string> matches, Action found)
		{
			var result = matches.Any(x => arg.ToUpper().Equals(x));
			if (result)
				found();
			return result;
		}

		private static bool matchSwitchWithAdjacentValue(string arg, string match, Action<string> value) =>
			matchSwitchWithAdjacentValue(arg, new[] {match}, value);

		private static bool matchSwitchWithAdjacentValue(string arg, IEnumerable<string> matches, Action<string> value)
		{
			var match = matches.OrderByDescending(x => x.Length)
				.FirstOrDefault(arg.StartsWith);

			if (match == null)
				return false;

			var v = arg.Remove(0, match.Length);
			value(v);
			return true;
		}
	}
}