using System;
using System.IO;
using System.Linq;

namespace Teleopti.Support.Code.Tool
{
	public class SsoConfigurationBackupHandler : ISupportCommand
	{
		private readonly CustomSection _customSection;
		private readonly SsoFilePathReader _ssoFilePathReader;

		public SsoConfigurationBackupHandler(CustomSection customSection, SsoFilePathReader ssoFilePathReader)
		{
			_customSection = customSection;
			_ssoFilePathReader = ssoFilePathReader;
		}

		public void Execute(ModeFile mode)
		{
			var ssoFilePath = _ssoFilePathReader.Read(mode);
			if (!ssoFilePath.IsValid())
			{
				return;
			}

			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SavedForSSOConfiguration");
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			
			var custom = new CustomSectionContainer();
			_customSection.Read(ssoFilePath.AuthBridgeConfig, custom);
			_customSection.Read(ssoFilePath.ClaimPolicies, custom);

			var config = File.ReadAllLines(ssoFilePath.WebConfig);
			foreach (var line in config.Where(line => line.Contains("DefaultIdentityProvider")))
			{
				custom.AddSection();
				custom.Add(line);
				break;
			}

			custom.WriteToFile(Path.Combine(path,"SSOConfiguration.txt"));
		}
	}
}