using System;
using System.IO;

namespace Teleopti.Support.Tool.Tool
{
	public class ConfigurationBackupCommand : ISupportCommand
	{
		private readonly CustomSection _customSection;
		private readonly ConfigFilePathReader _configFilePathReader;
		private readonly Func<ConfigFiles> _configFiles;

		public ConfigurationBackupCommand(CustomSection customSection, ConfigFilePathReader configFilePathReader, Func<ConfigFiles> configFiles)
		{
			_customSection = customSection;
			_configFilePathReader = configFilePathReader;
			_configFiles = configFiles;
		}

		public void Execute()
		{
			var ssoFilePath = _configFilePathReader.Read(_configFiles());
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
			_customSection.Read(ssoFilePath.AuthBridgeConfig, custom);
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

			_customSection.Read(ssoFilePath.ClaimPolicies, custom);

			var config = File.ReadAllLines(ssoFilePath.WebConfig);
			foreach (var line in config)
			{
				if (line.Contains("DefaultIdentityProvider") || line.Contains("Content-Security-Policy"))
				{
					custom.AddSection();
					custom.Add(line);
				}
			}

			custom.WriteToFile(Path.Combine(path,"Configurations.txt"));
		}
	}
}