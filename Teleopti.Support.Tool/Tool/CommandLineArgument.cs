﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Support.Library.Config;

namespace Teleopti.Support.Tool.Tool
{
	public interface ICommandLineArgument
	{
		ModeFile Mode { get; }
		ISupportCommand Command { get; }
	}

	public class CommandLineArgument : ICommandLineArgument
	{
		private readonly IEnumerable<string> _args;

		public CommandLineArgument(IEnumerable<string> args)
		{
			_args = args;
			readArguments();
		}

		public ISupportCommand Command { get; private set; }

		public ModeFile Mode { get; private set; }

		private string _helpText = @"Command line arguments:

		-? or ? or -HELP or HELP, Shows this help
		-MO[MODE] is where to put the config files values: Develop or Deploy (Develop is default)
			-BC Backup config settings for SSO and CSP ( before patching if there is custom settings)
			-RC Restore the config settings for SSO and CSP from the backup, combine with -MO (use after patching if there is custom settings)
		-TC[ALL|RC|CUSTOMER] Set ToggleMode to ALL, RC or CUSTOMER.
		-SET [searchFor] [replaceWith] Set a setting value
				Example: Teleopti.Support.Tool.exe -TC""ALL""";

		private void readArguments()
		{
			var helpCommand = new HelpWindow(_helpText);
			var restoreCommand = new ConfigurationRestoreHandler(new CustomSection(), new ConfigFilePathReader());
			var backupCommand = new ConfigurationBackupHandler(new CustomSection(), new ConfigFilePathReader());

			var modeCommonCommand = new CompositeCommand(
				new RefreshConfigsRunner(new RefreshConfigFile()),
				restoreCommand,
				new MoveCustomReportsCommand()
				);

			var modeDeployCommand = new CompositeCommand(
				new RefreshConfigsRunner(new RefreshConfigFile()),
				restoreCommand
				);

			Command = modeCommonCommand;

			_args
				.Select(a => a.ToUpper())
				.ForEach(argument =>
			{

				if (new[] { "-?", "?", "-H", "-HELP", "HELP" }.Contains(argument))
					Command = helpCommand;



				if (argument.StartsWith("-MO"))
				{
					Mode = new ModeFile("ConfigFiles.txt");
					Command = modeCommonCommand;
				}
				if (argument.StartsWith("-MODEPLOY"))
				{
					Mode = new ModeFile("DeployConfigFiles.txt");
					Command = modeDeployCommand;
				}
				if (argument.StartsWith("-MOAZURE"))
					Mode = new ModeFile("AzureConfigFiles.txt");
				if (argument.StartsWith("-MOTEST"))
					Mode = new ModeFile("BuildServerConfigFiles.txt");



				if (argument.StartsWith("-BC"))
				{
					Mode = new ModeFile("DeployConfigFiles.txt");
					Command = backupCommand;
				}
				if (argument.StartsWith("-RC"))
					Command = restoreCommand;



				if (argument.StartsWith("-TC"))
				{
					Mode = null;
					Command = new SetToggleModeCommand(argument.Remove(0, 3));
				}



				if (argument.Equals("-SET"))
					Command = new SetSettingCommand(_args.ElementAt(1), _args.ElementAt(2));
			});

		}

	}
}