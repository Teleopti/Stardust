using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Teleopti.Support.Library.Config
{
	public interface ISettingsFileManager
	{
		Tags ReadFile();
		void SaveFile(IList<SearchReplace> searchReplaces);
	}

	public class SettingsFileManager : ISettingsFileManager
	{
		private readonly string settingsFileName = "settings.txt";

		public Tags ReadFile()
		{
			var tags = new TextToTags()
				.ParseText(
					File.ReadAllText(findSettingsFileByBlackMagic(settingsFileName))
				);
			// this cant be good, but I wont change the behavior
			tags.FixSomeValuesAfterReading();
			return tags;
		}

		public void SaveFile(IList<SearchReplace> searchReplaces)
		{
			var path = findSettingsFileByBlackMagic(settingsFileName);
			var text = "";
			foreach (var searchReplace in searchReplaces)
			{
				text = text + searchReplace.SearchFor + "|" + searchReplace.ReplaceWith + Environment.NewLine;
			}
			File.WriteAllText(path, text);
		}

		private static string findSettingsFileByBlackMagic(string fileName)
		{
			var paths = new[]
			{
				@".\",
				@"..\",
				@"..\..\",
				@"..\..\..\",
				@"..\..\..\..\",
				@".\Teleopti.Support.Tool\bin\Debug\",
				@"..\Teleopti.Support.Tool\bin\Debug\",
				@"..\..\Teleopti.Support.Tool\bin\Debug\",
				@"..\..\..\Teleopti.Support.Tool\bin\Debug\",
				@"..\..\..\..\Teleopti.Support.Tool\bin\Debug\",
				@".\Teleopti.Support.Tool\bin\Release\",
				@"..\Teleopti.Support.Tool\bin\Release\",
				@"..\..\Teleopti.Support.Tool\bin\Release\",
				@"..\..\..\Teleopti.Support.Tool\bin\Release\",
				@"..\..\..\..\Teleopti.Support.Tool\bin\Release\",
			};
			return paths
				.Select(x => Path.Combine(x, fileName))
				.First(File.Exists);
		}
	}
}