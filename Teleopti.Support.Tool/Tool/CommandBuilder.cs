using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Support.Library.Config;

namespace Teleopti.Support.Tool.Tool
{
	public static class CommandBuilder
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(CommandBuilder));

		public static ISupportCommand ParseCommandLine(IEnumerable<string> args)
		{
			ISupportCommand command = null;
			ModeFile mode = null;

			var configFilePathReader = new ConfigFilePathReader();
			var customSection = new CustomSection();
			var restoreCommand = new ConfigurationRestoreCommand(customSection, configFilePathReader, () => mode);
			var backupCommand = new ConfigurationBackupCommand(customSection, configFilePathReader, () => mode);
			var refreshConfigFile = new RefreshConfigFile();
			var refreshConfigsRunner = new RefreshConfigsRunner(refreshConfigFile, () => mode);

			var modeDefaultCommand = new CompositeCommand(
				refreshConfigsRunner,
				restoreCommand,
				new MoveCustomReportsCommand()
			);

			var modeDeployCommand = new CompositeCommand(
				refreshConfigsRunner,
				restoreCommand
			);

			logger.InfoFormat("support tool is called with args: {0}", string.Join(" ", args));

			args
				.Select(a => a.ToUpper())
				.ForEach(argument =>
				{
					if (new[] {"-?", "?", "-H", "-HELP", "HELP"}.Contains(argument))
						command = new HelpWindow(HelpWindow.HelpText);

					if (argument.Equals("-MODEBUG"))
					{
						mode = new ModeFile("ConfigFiles.txt");
						command = modeDefaultCommand;
					}

					if (argument.Equals("-MODEPLOYWEB"))
					{
						mode = new ModeFile("DeployConfigFilesWeb.txt");
						command = modeDeployCommand;
					}

					if (argument.Equals("-MODEPLOYWORKER"))
					{
						mode = new ModeFile("DeployConfigFilesWorker.txt");
						command = modeDeployCommand;
					}

					if (argument.Equals("-MOAZURE"))
					{
						mode = new ModeFile("AzureConfigFiles.txt");
						command = modeDefaultCommand;
					}

					if (argument.Equals("-MOTEST"))
					{
						mode = new ModeFile("BuildServerConfigFiles.txt");
						command = modeDefaultCommand;
					}

					if (argument.Equals("-BC"))
					{
						mode = new ModeFile("DeployConfigFilesWeb.txt");
						command = backupCommand;
					}

					if (argument.Equals("-RC"))
						command = restoreCommand;

					if (argument.StartsWith("-TC"))
						command = new SetToggleModeCommand(argument.Remove(0, 3));
					if (argument.Equals("-SET"))
						command = new SetSettingCommand(args.ElementAt(1), args.ElementAt(2));
					if (argument.Equals("-CS"))
						command = new ConfigServerCommand(args.ElementAt(1));
					if (argument.Equals("-PM"))
						command = new SavePmConfigurationCommand(args.ElementAt(1));
				});

			return command;
		}
	}
}