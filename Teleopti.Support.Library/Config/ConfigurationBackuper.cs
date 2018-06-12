using System;
using System.IO;

namespace Teleopti.Support.Library.Config
{
	public class ConfigurationBackupCommand
	{
		public ConfigFiles ConfigFiles;
	}

	public class ConfigurationBackuper
	{
		public void Backup(ConfigurationBackupCommand command)
		{
			var customSection = new CustomSection();
			var configFilePathReader = new ConfigFilePathReader();
			var ssoFilePath = configFilePathReader.Read(command.ConfigFiles);
			if (!ssoFilePath.IsValid())
			{
				return;
			}

			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SavedForConfiguration");
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			var custom = new CustomSectionContainer();
			customSection.Read(ssoFilePath.AuthBridgeConfig, custom);
			var authConfig = File.ReadAllLines(ssoFilePath.AuthBridgeConfig);
			foreach (var line in authConfig)
			{
				if (line.Contains("Content-Security-Policy"))
				{
					custom.AddSection();
					custom.Add(line);
					break;
				}
			}

			customSection.Read(ssoFilePath.ClaimPolicies, custom);

			var config = File.ReadAllLines(ssoFilePath.WebConfig);
			foreach (var line in config)
			{
				if (line.Contains("DefaultIdentityProvider") || line.Contains("Content-Security-Policy"))
				{
					custom.AddSection();
					custom.Add(line);
				}
			}

			custom.WriteToFile(Path.Combine(path, "Configurations.txt"));
		}
	}
}