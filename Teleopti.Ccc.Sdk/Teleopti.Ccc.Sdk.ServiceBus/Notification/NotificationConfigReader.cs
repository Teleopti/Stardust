using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{
	public class NotificationConfigReader : INotificationConfigReader
	{
		private readonly string _configFile;
		private XmlDocument _configXml;

		public NotificationConfigReader()
			: this("NotificationConfig.xml")
		{ }

		public NotificationConfigReader(string configFile)
		{
			_configFile = configFile;
			loadFile();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
		public NotificationConfigReader(XmlDocument xmlDocument)
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
				
				var path = Path.Combine(dir, _configFile);
				if (File.Exists(path))
				{
					_configXml.Load(path);
				}
				else
				{
					_configXml = null;
					return;
				}
			}
			catch (FileNotFoundException)
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

		public string Assembly
		{
			get
			{
				if (!HasLoadedConfig)
					return "";
				if (_configXml.GetElementsByTagName("assembly").Count > 0)
					return _configXml.GetElementsByTagName("assembly")[0].InnerText;
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

		public string FindSuccessOrError
		{
			get
			{
				if (!HasLoadedConfig)
					return "";
				if (_configXml.GetElementsByTagName("FindSuccessOrError").Count > 0)
					return _configXml.GetElementsByTagName("FindSuccessOrError")[0].InnerText;
				return "";
			}
		}

		public string ErrorCode
		{
			get
			{
				if (!HasLoadedConfig)
					return "";
				if (_configXml.GetElementsByTagName("ErrorCode").Count > 0)
					return _configXml.GetElementsByTagName("ErrorCode")[0].InnerText;
				return "";
			}
		}

		public string SuccessCode
		{
			get
			{
				if (!HasLoadedConfig)
					return "";
				if (_configXml.GetElementsByTagName("SuccessCode").Count > 0)
					return _configXml.GetElementsByTagName("SuccessCode")[0].InnerText;
				return "";
			}
		}

		public bool SkipSearch
		{
			get
			{
				if (!HasLoadedConfig)
					return true;
				if (_configXml.GetElementsByTagName("SkipSearch").Count > 0)
					return Convert.ToBoolean(_configXml.GetElementsByTagName("SkipSearch")[0].InnerText);
				return false;
			}
		}

	    public virtual INotificationClient CreateClient()
	    {
	        return new NotificationWebClient(Url);
	    }
	}
}