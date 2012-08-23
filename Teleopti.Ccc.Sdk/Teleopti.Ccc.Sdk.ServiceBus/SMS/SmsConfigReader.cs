using System.Xml;

namespace Teleopti.Ccc.Sdk.ServiceBus.SMS
{
	public interface ISmsConfigReader
	{
		bool HasLoadedConfig { get; }
		XmlDocument XmlDocument { get; }
		string Url { get; }
		string User { get; }
		string Password { get; }
		string From { get; }
		string Class { get; }
		string Api { get; }
		string Data { get; }
	}

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
				_configXml.Load(_configFile);
			}
			catch (System.IO.FileNotFoundException)
			{
				_configXml = null;
				return;
			}
			HasLoadedConfig = true;
		}

		public bool HasLoadedConfig { get; private set; }

		public XmlDocument XmlDocument
		{
			get { return _configXml; }
		}

		public string Url
		{
			get
			{
				if (!HasLoadedConfig)
					return "";
				if (_configXml.GetElementsByTagName("url").Count > 0)
					return _configXml.GetElementsByTagName("url")[0].InnerText;
				return "";
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

		public string Class
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