using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Teleopti.Support.Tool.Tool
{
	public class ConfigurationRestoreCommand : ISupportCommand
	{
		private readonly CustomSection _customSection;
		private readonly ConfigFilePathReader _configFilePathReader;
		private readonly Func<ModeFile> _mode;

		public ConfigurationRestoreCommand(CustomSection customSection, ConfigFilePathReader configFilePathReader, Func<ModeFile> mode)
		{
			_customSection = customSection;
			_configFilePathReader = configFilePathReader;
			_mode = mode;
		}

		public void Execute()
		{
			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SavedForConfiguration");
			var backupFile = Path.Combine(path, "Configurations.txt");
			if (!File.Exists(backupFile))
			{
				return;
			}

			var configFilePaths = _configFilePathReader.Read(_mode());
			if (!configFilePaths.IsValid())
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
						_customSection.WriteCustomSection(item.Key, configFilePaths.AuthBridgeConfig, item.Value);
						break;
					case 3:
						writeCustomRow(configFilePaths.AuthBridgeConfig, "Content-Security-Policy", item.Value);
						break;
					case 4:
					case 5:
					case 6:
						_customSection.WriteCustomSection(item.Key - 3, configFilePaths.ClaimPolicies, item.Value);
						break;
					case 7:
						writeCustomRow(configFilePaths.WebConfig, "DefaultIdentityProvider", item.Value);
						break;
					case 8:
						writeCustomRow(configFilePaths.WebConfig, "Content-Security-Policy", item.Value);
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

		private void writeCustomRow(string filePath, string key, IEnumerable<string> towrite)
		{
			var config = File.ReadAllLines(filePath).ToList();

			var sectionRowStart = config.Select((x, i) => new {x, i}).First(line => line.x.Contains(key));
			config.RemoveAt(sectionRowStart.i);
			config.InsertRange(sectionRowStart.i, towrite);

			File.WriteAllLines(filePath, config);
		}
	}
}