using System;
using System.IO;

namespace Teleopti.Support.Tool.Tool
{
	public class ConfigurationBackupHandler : ISupportCommand
	{
		private readonly CustomSection _customSection;
		private readonly ConfigFilePathReader _configFilePathReader;

		public ConfigurationBackupHandler(CustomSection customSection, ConfigFilePathReader configFilePathReader)
		{
			_customSection = customSection;
			_configFilePathReader = configFilePathReader;
		}

		public void Execute(ModeFile mode)
		{
			var ssoFilePath = _configFilePathReader.Read(mode);
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