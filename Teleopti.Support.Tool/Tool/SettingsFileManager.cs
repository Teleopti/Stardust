using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace Teleopti.Support.Tool.Tool
{
	public interface ISettingsFileManager
	{
		IList<SearchReplace> GetReplaceList();
		void SaveReplaceList(IList<SearchReplace> searchReplaces);
	}

	public class SettingsFileManager : ISettingsFileManager
	{

		private readonly SettingsReader _reader;

		public SettingsFileManager(SettingsReader reader)
		{
			_reader = reader;
		}

		public IList<SearchReplace> GetReplaceList()
		{
			var searchReplaceListInSettingsFile = _reader.GetSearchReplaceList(File.ReadAllText(CurrentPath()));
			var hostName = searchReplaceListInSettingsFile.SingleOrDefault(x => x.SearchFor == "$(HOST_NAME)");
			var dnsAlias = searchReplaceListInSettingsFile.SingleOrDefault(x => x.SearchFor == "$(DNS_ALIAS)");
			var stardust = searchReplaceListInSettingsFile.SingleOrDefault(x => x.SearchFor == "$(STARDUST)");
			if (hostName != null && dnsAlias != null)
			{
				hostName.ReplaceWith = dnsAlias.ReplaceWith.Replace(@"http://", "").Replace(@"https://", "").TrimEnd('/');
				if (stardust != null)
					stardust.ReplaceWith = stardust.ReplaceWith.Replace(stardust.ReplaceWith, dnsAlias.ReplaceWith);
			}
			if (stardust != null)
				stardust.ReplaceWith = stardust.ReplaceWith.TrimEnd('/');
			return searchReplaceListInSettingsFile;
		}

		public void SaveReplaceList(IList<SearchReplace> searchReplaces)
		{
			var path = CurrentPath();
			var text = "";
			foreach (var searchReplace in searchReplaces)
			{
				text = text + searchReplace.SearchFor + "|" + searchReplace.ReplaceWith + Environment.NewLine;
			}
			File.WriteAllText(path, text);
		}

		private static string CurrentPath()
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