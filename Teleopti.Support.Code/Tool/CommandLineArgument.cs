using System;
using System.Globalization;

namespace Teleopti.Support.Code.Tool
{
	public interface ICommandLineArgument
	{
		ModeFile Mode { get; }
		ISupportCommand Command { get; }
	}

	public class CommandLineArgument : ICommandLineArgument
	{
		private readonly string[] _argumentCollection;
		private readonly Func<string, ISupportCommand> _helpCommand;

		public CommandLineArgument(string[] argumentCollection, Func<string, ISupportCommand> helpCommand)
		{
			_argumentCollection = argumentCollection;
			_helpCommand = helpCommand;
			ReadArguments();
		}

		public ISupportCommand Command { get; private set; }

		public ModeFile Mode { get; private set; }

		public string Help
		{
			get
			{
				return @"Command line arguments:

		-? or ? or -HELP or HELP, Shows this help
		-MO  (Mode )is where to put the config files values: Develop or Deploy (Develop is default)
		-BC Backup config settings for SSO, combine with -MO ( before patching if there is custom settings)
		-RC Restore the config settings for SSO from the backup, combine with -MO (use after patching if there is custom settings)";
			}
		}

		private void ReadArguments()
		{
			var restoreCommand = new SsoConfigurationRestoreHandler(new CustomSection(), new SsoFilePathReader());
			Command = new CompositeCommand(new RefreshConfigsRunner(new SettingsFileManager(new SettingsReader()),
				new RefreshConfigFile(new ConfigFileTagReplacer(),
					new MachineKeyChecker())),restoreCommand);
			foreach (string s in _argumentCollection)
			{
				switch (s)
				{
					case "-?":
					case "?":
					case "-HELP":
					case "HELP":
						Command = _helpCommand.Invoke(Help);
						continue;
				}

				string switchType = s.Substring(0, 3).ToUpper(CultureInfo.CurrentCulture);
				string switchValue = s.Remove(0, 3);

				switch (switchType)
				{
					case "-MO": // Mode.
						Mode = new ModeFile(switchValue);
						break;
					case "-BC":
						Command = new SsoConfigurationBackupHandler(new CustomSection(), new SsoFilePathReader());
						break;
					case "-RC":
						Command = restoreCommand;
						break;

				}
			}
		}

	}
}