using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Teleopti.Support.Library.Config
{
	public class SearchReplace
	{
		public SearchReplace()
		{
		}

		public SearchReplace(string searchFor, string replaceWith)
		{
			SearchFor = searchFor;
			ReplaceWith = replaceWith;
		}

		public string SearchFor { get; set; }
		public string ReplaceWith { get; set; }
	}

	public class Tags
	{
		private readonly IList<SearchReplace> _searchReplaces = new List<SearchReplace>();

		public void AddSearchReplace(string searchFor, string replaceWith)
		{
			_searchReplaces.Add(new SearchReplace(searchFor, replaceWith));
		}

		public void AddTag(string tag, string replaceWith)
		{
			addSimple(tag, replaceWith);
			addAsXmlComment(tag, replaceWith);
			addAsXmlAppSetting(tag, replaceWith);
		}

		private void addSimple(string tag, string replaceWith)
		{
			var searchFor = string.Format("$({0})", tag);
			AddSearchReplace(searchFor, replaceWith);
		}

		private void addAsXmlComment(string tag, string replaceWith)
		{
			var searchFor = string.Format("<!--$({0})-->", tag);
			AddSearchReplace(searchFor, replaceWith);
		}

		private void addAsXmlAppSetting(string tag, string replaceWith)
		{
			var searchFor = string.Format("<!--$({0}AppSetting)-->", tag);
			replaceWith = string.Format(@"<add key=""{0}"" value=""{1}"" />", tag, replaceWith);
			AddSearchReplace(searchFor, replaceWith);
		}

		public void FixSomeValuesAfterReading()
		{
			var searchReplaceListInSettingsFile = _searchReplaces;

			// this cant be good, but I wont change the behavior
			//(250ab543c41c) #31416 Cannot start MyTime (or any web) after upgrade to v8
			//(36c177cd2926) hope this will do it for Stardust when the installing
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
		}

		public IEnumerable<SearchReplace> ForDisplay()
		{
			return _searchReplaces;
		}

		public IEnumerable<SearchReplace> ForReplacing()
		{
			// this cant be good, but I wont change the behavior
			// (c150de6ca27c) #31623 Support tool crashes during installation if the impersonation password includes a special character

			foreach (var searchReplace in _searchReplaces)
			{
				var replaceWith = searchReplace.ReplaceWith;
				if (searchReplace.SearchFor == "$(PM_ANONYMOUS_PWD)")
				{
					yield return new SearchReplace(searchReplace.SearchFor, xmlEscape(replaceWith));
				}
				else
				{
					yield return searchReplace;
				}
			}
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