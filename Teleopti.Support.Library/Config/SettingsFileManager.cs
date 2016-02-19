using System;
using System.Collections.Generic;
using System.IO;

namespace Teleopti.Support.Library.Config
{
	public interface ISettingsFileManager
	{
		Tags GetTags();
		void SaveTagAndValues(IList<SearchReplace> searchReplaces);
	}

	public class SettingsFileManager : ISettingsFileManager
	{

		private readonly TextToTags _textToTags;

		public SettingsFileManager(TextToTags textToTags)
		{
			_textToTags = textToTags;
		}

		public Tags GetTags()
		{
			var tags = _textToTags.ParseText(File.ReadAllText(findSettingsFileByBlackMagic()));
			// this cant be good, but I wont change the behavior
			tags.FixSomeValuesAfterReading();
			return tags;
		}

		public void SaveTagAndValues(IList<SearchReplace> searchReplaces)
		{
			var path = findSettingsFileByBlackMagic();
			var text = "";
			foreach (var searchReplace in searchReplaces)
			{
				text = text + searchReplace.SearchFor + "|" + searchReplace.ReplaceWith + Environment.NewLine;
			}
			File.WriteAllText(path, text);
		}

		private static string findSettingsFileByBlackMagic()
		{
			var directoryInfo = Directory.GetParent(Directory.GetCurrentDirectory()).Parent;
			string path = "";
			if (directoryInfo != null)
			{
				var dir = directoryInfo.FullName;
				//debug environment
				path = Path.Combine(dir, @"..\Teleopti.Support.Code\settings.txt");
				//Release (and tests)
				if (!File.Exists(path))
					path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"settings.txt");
			}
			if (path == "")
				path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"settings.txt");
			return path;
		}
	}
}