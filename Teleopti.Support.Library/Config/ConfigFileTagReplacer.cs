using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Teleopti.Support.Library.Config
{
	public interface IConfigFileTagReplacer
	{
		void ReplaceTags(string fileToProcess, IList<SearchReplace> searchReplaces);
	}

	public class ConfigFileTagReplacer : IConfigFileTagReplacer
	{
		public void ReplaceTags(string fileToProcess, IList<SearchReplace> searchReplaces)
		{
			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileToProcess);
			var text = File.ReadAllText(path);
			foreach (var searchReplace in searchReplaces)
			{
				var replaceWith = searchReplace.ReplaceWith;
				if (searchReplace.SearchFor == "$(PM_ANONYMOUS_PWD)")
				{
					replaceWith = xmlEscape(replaceWith);
				}
				text = text.Replace(searchReplace.SearchFor, replaceWith);
			}
			File.WriteAllText(path, text);
		}

		private static string xmlEscape(string unescaped)
		{
			var doc = new XmlDocument();
			var node = doc.CreateElement("root");
			node.InnerText = unescaped;
			return node.InnerXml;
		}
	}
}