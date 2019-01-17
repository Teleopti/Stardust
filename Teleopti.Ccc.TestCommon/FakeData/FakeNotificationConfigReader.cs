using System;
using System.Xml;
using System.Xml.XPath;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeNotificationConfigReader : INotificationConfigReader
	{
		private XmlDocument _configXml;
		public bool HasLoadedConfig => XmlDocument != null;
		public IXPathNavigable XmlDocument => _configXml;
		public Uri Url { get; set; }
		public string User { get; }
		public string Password { get; }
		public string From { get; }
		public string ClassName { get; }
		public string Assembly { get; }
		public string Api { get; }
		public string Data => getValue("data");
		public string FindSuccessOrError => getValue("FindSuccessOrError");
		public string ErrorCode => getValue("ErrorCode");
		public string SuccessCode => getValue("SuccessCode");
		public bool SkipSearch
		{
			get
			{
				if (!HasLoadedConfig)
					return true;
				var skipSearch = _configXml.GetElementsByTagName("SkipSearch");
				if (skipSearch.Count > 0)
					return Convert.ToBoolean(skipSearch[0].InnerText);
				return false;
			}
		}
		public bool SmtpEnabled => HasLoadedConfig && SmtpHost.Length > 0 && SmtpPort > -1;
		public string SmtpHost { get; }
		public int SmtpPort { get; }
		public bool SmtpUseSsl { get; }
		public string SmtpUser { get; }
		public string SmtpPassword { get; }
		public bool SmtpUseRelay { get; }

		public void LoadConfig(string xmlConfig)
		{
			_configXml = new XmlDocument();
			_configXml.LoadXml(xmlConfig);
		}

		private string getValue(string tagName) {
			if (!HasLoadedConfig)
				return string.Empty;
			var tag = _configXml.GetElementsByTagName(tagName);
			if (tag.Count > 0)
				return tag[0].InnerText;
			return string.Empty;
		}

		public string ContentType { get; set; }
		public string EncodingName { get; set; }
	}
}
