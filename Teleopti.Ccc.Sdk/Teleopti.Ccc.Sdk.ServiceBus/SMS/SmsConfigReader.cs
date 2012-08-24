using System;
using System.Xml;
using System.Xml.XPath;

namespace Teleopti.Ccc.Sdk.ServiceBus.SMS
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms")]
	public interface ISmsConfigReader
	{
		bool HasLoadedConfig { get; }
		IXPathNavigable XmlDocument { get; }
		Uri Url { get; }
		string User { get; }
		string Password { get; }
		string From { get; }
		string ClassName { get; }
		string Api { get; }
		string Data { get; }
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms")]
	public class SmsConfigReader : ISmsConfigReader
	{
		private readonly string _configFile;
		private XmlDocument _configXml;

		public SmsConfigReader()
			: this("SmsConfig.xml")
		{ }

		public SmsConfigReader(string configFile)
		{
			_configFile = configFile;
			loadFile();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
		public SmsConfigReader(XmlDocument xmlDocument)
		{
			_configXml = xmlDocument;
			HasLoadedConfig = true;
		}

		private void loadFile()
		{
			_configXml = new XmlDocument();
			try
			{
				var dir = AppDomain.CurrentDomain.BaseDirectory;
				if (!dir.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
					dir = dir + "\\";
				_configXml.Load(dir + _configFile);
			}
			catch (System.IO.FileNotFoundException)
			{
				_configXml = null;
				return;
			}
			HasLoadedConfig = true;
		}

		public bool HasLoadedConfig { get; private set; }

		public IXPathNavigable XmlDocument
		{
			get { return _configXml; }
		}

		public Uri Url
		{
			get
			{
				if (!HasLoadedConfig)
					return null;
				if (_configXml.GetElementsByTagName("url").Count > 0)
					return new Uri(_configXml.GetElementsByTagName("url")[0].InnerText);
				return null;
			}
		}

		public string User
		{
			get
			{
				if (!HasLoadedConfig)
					return "";
				if (_configXml.GetElementsByTagName("user").Count > 0)
					return _configXml.GetElementsByTagName("user")[0].InnerText;
				return "";
			}
		}

		public string Password
		{
			get
			{
				if (!HasLoadedConfig)
					return "";
				if (_configXml.GetElementsByTagName("password").Count > 0)
					return _configXml.GetElementsByTagName("password")[0].InnerText;
				return "";
			}
		}

		public string From
		{
			get
			{
				if (!HasLoadedConfig)
					return "";
				if (_configXml.GetElementsByTagName("from").Count > 0)
					return _configXml.GetElementsByTagName("from")[0].InnerText;
				return "";
			}
		}

		public string ClassName
		{
			get
			{
				if (!HasLoadedConfig)
					return "";
				if (_configXml.GetElementsByTagName("class").Count > 0)
					return _configXml.GetElementsByTagName("class")[0].InnerText;
				return "";
			}
		}

		public string Api
		{
			get
			{
				if (!HasLoadedConfig)
					return "";
				if (_configXml.GetElementsByTagName("api_id").Count > 0)
					return _configXml.GetElementsByTagName("api_id")[0].InnerText;
				return "";
			}
		}

		public string Data
		{
			get
			{
				if (!HasLoadedConfig)
					return "";
				if (_configXml.GetElementsByTagName("data").Count > 0)
					return _configXml.GetElementsByTagName("data")[0].InnerText;
				return "";
			}
		}
	}
}