using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Teleopti.Support.Code.Tool
{
	public class SsoConfigurationRestoreHandler
	{
		public void Excute(ModeFile mode)
		{
			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SavedForSSOConfiguration");
			var backupFile = Path.Combine(path, "SSOConfiguration.txt");
			if (!File.Exists(backupFile))
			{
				return;
			}

			var content = File.ReadAllLines(backupFile);
			var counter = 0;

			var dic = new Dictionary<int, List<string>>();

			foreach (var line in content)
			{
				int number;
				if (int.TryParse(line, out number))
				{
					counter = number;
					dic.Add(counter,new List<string>());
					continue;
				}
				dic[counter].Add(line);
			}


			var file = mode.FileContents();
			var authenticationBridgeConfig = filePath(file, "AuthenticationBridge");
			var directory = Path.GetDirectoryName(authenticationBridgeConfig);
			var claimsPoliciesXml = Path.Combine(directory, "App_Data\\claimsPolicies.xml");
			var webRootConfig = filePath(file, "Web.Root.Web.config");
			foreach (var item in dic)
			{
				switch (item.Key)
				{
					case 1:
					case 2:
						writeCustomSection(item.Key, authenticationBridgeConfig, item.Value);
						break;
					case 3:
					case 4:
					case 5:
						writeCustomSection(item.Key - 2, claimsPoliciesXml, item.Value);
						break;
					/*case 6:
						writeCustomSection(item.Key, webRootConfig, item.Value);
						break;*/
					default:
						break;
				}
			}
			
		}

		private static void writeCustomSection(int section, string filePath, IEnumerable<string> towrite)
		{
			var config = File.ReadAllLines(filePath).ToList();
			var test = config.Where(c => c.Contains("<!-- custom section starts here -->"));
			var sectionRowStart = config.Select((x,i) => new {x,i}).Where(c => c.x.Contains("<!-- custom section starts here -->")).ElementAt(section - 1);
			var sectionRowEnd = config.Select((x, i) => new { x, i }).Where(c => c.x.Contains("<!-- custom section ends here -->")).ElementAt(section - 1);
			var indexRowEnd = sectionRowEnd.i;
			var start = sectionRowStart.i + 1;
			var rowsToRemove = indexRowEnd - start;

			for (int i = indexRowEnd-1; i>=indexRowEnd-rowsToRemove; i--)
			{
				config.RemoveAt(i);
			}
			
			config.InsertRange(start,towrite);
			File.WriteAllLines(filePath, config);
		}

		private static string filePath(IEnumerable<string> file, string searchFor)
		{
			var configFileLine = file.FirstOrDefault(f => f.Contains(searchFor));
			if (string.IsNullOrEmpty(configFileLine))
			{
				return null;
			}

			return configFileLine.Substring(0, configFileLine.IndexOf(','));
		}
	}
	public class SsoConfigurationBackupHandler
	{
		public void Excute(ModeFile mode)
		{
			var file = mode.FileContents();
			var authenticationBridgeConfig = file.FirstOrDefault(f => f.Contains("AuthenticationBridge"));
			if (string.IsNullOrEmpty(authenticationBridgeConfig))
			{
				return;
			}

			var webConfigForAuthBridge = authenticationBridgeConfig.Substring(0,authenticationBridgeConfig.IndexOf(','));
			
			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SavedForSSOConfiguration");
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			var tosave = new List<string>();
			var sectionCounter = 0;

			var directory = Path.GetDirectoryName(webConfigForAuthBridge);
			var claimsPoliciesXml = Path.Combine(directory, "App_Data\\claimsPolicies.xml");

			getCustomSection(ref sectionCounter, webConfigForAuthBridge, tosave);
			getCustomSection(ref sectionCounter, claimsPoliciesXml, tosave);

			var webRootConfigLine = file.FirstOrDefault(f => f.Contains("Web.Root.Web.config"));
			if (string.IsNullOrEmpty(webRootConfigLine))
			{
				return;
			}

			var webRootConfig = webRootConfigLine.Substring(0, webRootConfigLine.IndexOf(','));
			var config = File.ReadAllLines(webRootConfig);
			foreach (var line in config.Where(line => line.Contains("DefaultIdentityProvider")))
			{
				sectionCounter ++;
				tosave.Add((sectionCounter).ToString(CultureInfo.InvariantCulture));
				tosave.Add(line);
				break;
			}

			File.WriteAllLines(Path.Combine(path,"SSOConfiguration.txt"),tosave);
		}

		private static void getCustomSection(ref int counter, string filePath, List<string> tosave)
		{
			var config = File.ReadAllLines(filePath);
			var write = false;
			foreach (var line in config)
			{
				if (line.Contains("<!-- custom section ends here -->"))
				{
					write = false;
					continue;
				}
				if (write)
					tosave.Add(line);
				if (line.Contains("<!-- custom section starts here -->"))
				{
					counter ++;
					tosave.Add(counter.ToString(CultureInfo.InvariantCulture));
					write = true;
				}
			}
		}
	}
}