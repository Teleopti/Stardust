using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Teleopti.Support.Code.Tool
{
	public class SsoConfigurationRestoreHandler : ISupportCommand
	{
		private readonly CustomSection _customSection;
		private readonly SsoFilePathReader _ssoFilePathReader;

		public SsoConfigurationRestoreHandler(CustomSection customSection, SsoFilePathReader ssoFilePathReader)
		{
			_customSection = customSection;
			_ssoFilePathReader = ssoFilePathReader;
		}

		public void Execute(ModeFile mode)
		{
			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SavedForSSOConfiguration");
			var backupFile = Path.Combine(path, "SSOConfiguration.txt");
			if (!File.Exists(backupFile))
			{
				return;
			}

			var ssoFilePaths = _ssoFilePathReader.Read(mode);
			if (!ssoFilePaths.IsValid())
			{
				return;
			}

			var dic = loadFromBackupFile(backupFile);
			foreach (var item in dic)
			{
				switch (item.Key)
				{
					case 1:
					case 2:
						_customSection.WriteCustomSection(item.Key, ssoFilePaths.AuthBridgeConfig, item.Value);
						break;
					case 3:
					case 4:
					case 5:
						_customSection.WriteCustomSection(item.Key - 2, ssoFilePaths.ClaimPolicies, item.Value);
						break;
					case 6:
						writeCustomRow(ssoFilePaths.WebConfig, item.Value);
						break;
				}
			}
		}

		private static Dictionary<int, List<string>> loadFromBackupFile(string backupFile)
		{
			var content = File.ReadAllLines(backupFile);
			var counter = 0;

			var dic = new Dictionary<int, List<string>>();

			foreach (var line in content)
			{
				int number;
				if (int.TryParse(line, out number))
				{
					counter = number;
					dic.Add(counter, new List<string>());
					continue;
				}
				dic[counter].Add(line);
			}
			return dic;
		}

		private void writeCustomRow(string filePath, IEnumerable<string> towrite)
		{
			var config = File.ReadAllLines(filePath).ToList();
			var sectionRowStart = config.Select((x, i) => new {x, i}).First(line => line.x.Contains("DefaultIdentityProvider"));
			config.RemoveAt(sectionRowStart.i);
			
			config.InsertRange(sectionRowStart.i,towrite);
			File.WriteAllLines(filePath, config);
		}
	}
}