using System;
using System.Collections.Generic;
using System.IO;

namespace Teleopti.Support.Library.Config
{
	public class SettingsFileManager
	{
		public void LoadFile(string file) => SaveFile(readFileAndFixSomeMagicStuff(file).ForDisplay());

		public SearchReplaceCollection ReadFile() => readFileAndFixSomeMagicStuff(settingsFile());

		private static SearchReplaceCollection readFileAndFixSomeMagicStuff(string file)
		{
			var collection = new Parser().ParseText(File.ReadAllText(file));
			// this cant be good, but I wont change the behavior
			collection.FixSomeValuesAfterReading();
			return collection;
		}

		public void SaveFile(IEnumerable<SearchReplace> collection)
		{
			var text = "";
			foreach (var searchReplace in collection)
				text = text + searchReplace.SearchFor + "|" + searchReplace.ReplaceWith + Environment.NewLine;
			File.WriteAllText(settingsFile(), text);
		}
		
		private static string settingsFile() => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.txt");
		
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

		
	}
}