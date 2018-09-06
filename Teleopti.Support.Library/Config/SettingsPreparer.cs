using System.IO;
using Teleopti.Support.Library.Folders;

namespace Teleopti.Support.Library.Config
{
	public class SettingPrepareCommand
	{
		public string Server;
		public string ApplicationDatabase;
		public string AnalyticsDatabase;
		public string ToggleMode = "ALL";
		public string SqlAuthString;
		public string MachineKeyValidationKey;
		public string MachineKeyDecryptionKey;
	}

	public class SettingsPreparer
	{
		public void Prepare(SettingPrepareCommand command)
		{
			new SettingsLoader().LoadSettingsFile(new LoadSettingsCommand
			{
				File = Path.Combine(new RepositoryRootFolder().Path(), @".debug-Setup\config\", "settings.txt")
			});

			var setter = new SettingsSetter();

			setter.UpdateSettingsFile(
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

			if (!string.IsNullOrEmpty(command.MachineKeyValidationKey))
			{
				setter.UpdateSettingsFile(
					new SetSettingCommand
					{
						SearchFor = "$(machineKey.validationKey)",
						ReplaceWith = command.MachineKeyValidationKey
					},
					new SetSettingCommand
					{
						SearchFor = "$(machineKey.decryptionKey)",
						ReplaceWith = command.MachineKeyDecryptionKey
					}
				);
			}

			if (!string.IsNullOrEmpty(command.Server))
			{
				setter.UpdateSettingsFile(
					new SetSettingCommand
					{
						SearchFor = "$(AS_SERVER_NAME)",
						ReplaceWith = command.Server
					},
					new SetSettingCommand
					{
						SearchFor = "$(SQL_SERVER_NAME)",
						ReplaceWith = command.Server
					},
					new SetSettingCommand
					{
						SearchFor = "$(SQL_AUTH_STRING)",
						ReplaceWith = $"Data Source={command.Server};Integrated Security=SSPI"
					}
				);
			}

			if (!string.IsNullOrEmpty(command.SqlAuthString))
				setter.UpdateSettingsFile(
					new SetSettingCommand
					{
						SearchFor = "$(SQL_AUTH_STRING)",
						ReplaceWith = command.SqlAuthString
					});
		}
	}
}