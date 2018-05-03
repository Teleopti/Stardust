using System.IO;
using Teleopti.Support.Library.Folders;

namespace Teleopti.Support.Tool.Tool
{
	public class FixMyConfigCommand
	{
		public string ApplicationDatabase;
		public string AnalyticsDatabase;
	}

	public class FixMyConfigFixer
	{
		public void Fix(FixMyConfigCommand command)
		{
			new SettingsLoader().LoadSettingsFile(new LoadSettingsCommand
			{
				File = Path.Combine(new RepositoryRootFolder().Path(), @".debug-Setup\config\", "settings.txt")
			});
			new SettingsSetter().UpdateSettingsFile(new SetSettingCommand
				{
					SearchFor = "machineKey.validationKey",
					ReplaceWith = "754534E815EF6164CE788E521F845BA4953509FA45E321715FDF5B92C5BD30759C1669B4F0DABA17AC7BBF729749CE3E3203606AB49F20C19D342A078A3903D1"
				},
				new SetSettingCommand
				{
					SearchFor = "machineKey.decryptionKey",
					ReplaceWith = "3E1AD56713339011EB29E98D1DF3DBE1BADCF353938C3429"
				},
				new SetSettingCommand
				{
					SearchFor = "DB_CCC7",
					ReplaceWith = command.ApplicationDatabase
				},
				new SetSettingCommand
				{
					SearchFor = "DB_ANALYTICS",
					ReplaceWith = command.AnalyticsDatabase
				},
				new SetSettingCommand
				{
					SearchFor = "AS_DATABASE",
					ReplaceWith = command.AnalyticsDatabase
				},
				new SetSettingCommand
				{
					SearchFor = "ToggleMode",
					ReplaceWith = "ALL"
				}
			);
			new ModeDebugRunner()
				.Run(new ModeDebugCommand());
		}
	}
}