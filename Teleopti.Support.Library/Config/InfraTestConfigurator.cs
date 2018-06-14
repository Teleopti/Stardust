using System.IO;
using Teleopti.Support.Library.Folders;

namespace Teleopti.Support.Tool.Tool
{
	public class InfraTestConfigCommand
	{
		public string ApplicationDatabase = "InfraTest_CCC7";
		public string AnalyticsDatabase = "InfraTest_Analytics";
		public string ToggleMode = "ALL";
		public string SqlAuthString;
	}

	public class InfraTestConfigurator
	{
		public void Configure(InfraTestConfigCommand command)
		{
			new SettingsLoader().LoadSettingsFile(new LoadSettingsCommand
			{
				File = Path.Combine(new RepositoryRootFolder().Path(), @".debug-Setup\config\", "settings.txt")
			});

			var manager = new SettingsSetter();

			manager.UpdateSettingsFile(
				new SetSettingCommand
				{
					SearchFor = "$(DB_CCC7)",
					ReplaceWith = command.ApplicationDatabase
				},
				new SetSettingCommand
				{
					SearchFor = "$(DB_ANALYTICS)",
					ReplaceWith = command.AnalyticsDatabase
				},
				new SetSettingCommand
				{
					SearchFor = "$(AS_DATABASE)",
					ReplaceWith = command.AnalyticsDatabase
				},
				new SetSettingCommand
				{
					SearchFor = "$(ToggleMode)",
					ReplaceWith = command.ToggleMode
				}
			);

			if (!string.IsNullOrEmpty(command.SqlAuthString))
				manager.UpdateSettingsFile(
					new SetSettingCommand
					{
						SearchFor = "$(SQL_AUTH_STRING)",
						ReplaceWith = command.SqlAuthString
					});

			new ModeTestRunner()
				.Run(new ModeTestCommand());
		}
	}
}