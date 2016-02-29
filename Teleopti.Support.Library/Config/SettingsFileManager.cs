using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Teleopti.Support.Library.Config
{
	public class SettingsFileManager
	{
		private readonly string settingsFileName = "settings.txt";

		public SearchReplaceCollection ReadFile()
		{
			var collection = new Parser()
				.ParseText(
					File.ReadAllText(findSettingsFileByBlackMagic(settingsFileName))
				);
			// this cant be good, but I wont change the behavior
			collection.FixSomeValuesAfterReading();
			return collection;
		}

		public void SaveFile(IEnumerable<SearchReplace> collection)
		{
			var path = findSettingsFileByBlackMagic(settingsFileName);
			var text = "";
			foreach (var searchReplace in collection)
			{
				text = text + searchReplace.SearchFor + "|" + searchReplace.ReplaceWith + Environment.NewLine;
			}
			File.WriteAllText(path, text);
		}

		public void UpdateFileByName(string name, string replaceWith)
		{
			var collection = ReadFile();
			collection.SetByName(name, replaceWith);
			SaveFile(collection.ForDisplay());
		}

		public void UpdateFile(string searchFor, string replaceWith)
		{
			var collection = ReadFile();
			collection.Set(searchFor, replaceWith);
			SaveFile(collection.ForDisplay());
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