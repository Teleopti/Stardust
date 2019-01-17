using System;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	public class NotificationConfigReader : INotificationConfigReader
	{
		private readonly string _configFile;
		private XmlDocument _configXml;

		public NotificationConfigReader()
			: this(ConfigurationManager.AppSettings["ConfigurationFilesPath"] + "/NotificationConfig.xml")
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

		public IXPathNavigable XmlDocument => _configXml;

		public Uri Url
		{
			get
			{
				if (!HasLoadedConfig)
					return null;
				var url = _configXml.GetElementsByTagName("url");
				if (url.Count > 0)
					return new Uri(url[0].InnerText);
				return null;
			}
		}

		public string User
		{
			get
			{
				if (!HasLoadedConfig)
					return "";
				var user = _configXml.GetElementsByTagName("user");
				if (user.Count > 0)
					return user[0].InnerText;
				return "";
			}
		}

		public string Password
		{
			get
			{
				if (!HasLoadedConfig)
					return "";
				var password = _configXml.GetElementsByTagName("password");
				if (password.Count > 0)
					return password[0].InnerText;
				return "";
			}
		}

		public string From
		{
			get
			{
				if (!HasLoadedConfig)
					return "";
				var from = _configXml.GetElementsByTagName("from");
				if (from.Count > 0)
					return from[0].InnerText;
				return "";
			}
		}

		public string ClassName
		{
			get
			{
				if (!HasLoadedConfig)
					return "";
				var clazz = _configXml.GetElementsByTagName("class");
				if (clazz.Count > 0)
					return clazz[0].InnerText;
				return "";
			}
		}

		public string Assembly
		{
			get
			{
				if (!HasLoadedConfig)
					return "";
				var assembly = _configXml.GetElementsByTagName("assembly");
				if (assembly.Count > 0)
					return assembly[0].InnerText;
				return "";
			}
		}

		public string Api
		{
			get
			{
				if (!HasLoadedConfig)
					return "";
				var apiId = _configXml.GetElementsByTagName("api_id");
				if (apiId.Count > 0)
					return apiId[0].InnerText;
				return "";
			}
		}

		public string Data
		{
			get
			{
				if (!HasLoadedConfig)
					return "";
				var data = _configXml.GetElementsByTagName("data");
				if (data.Count > 0)
					return data[0].InnerText;
				return "";
			}
		}

		public string FindSuccessOrError
		{
			get
			{
				if (!HasLoadedConfig)
					return "";
				var findSuccessOrError = _configXml.GetElementsByTagName("FindSuccessOrError");
				if (findSuccessOrError.Count > 0)
					return findSuccessOrError[0].InnerText;
				return "";
			}
		}

		public string ErrorCode
		{
			get
			{
				if (!HasLoadedConfig)
					return "";
				var errorCode = _configXml.GetElementsByTagName("ErrorCode");
				if (errorCode.Count > 0)
					return errorCode[0].InnerText;
				return "";
			}
		}

		public string SuccessCode
		{
			get
			{
				if (!HasLoadedConfig)
					return "";
				var successCode = _configXml.GetElementsByTagName("SuccessCode");
				if (successCode.Count > 0)
					return successCode[0].InnerText;
				return "";
			}
		}

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

		public string SmtpHost
		{
			get
			{
				if (!HasLoadedConfig)
					return "";
				var smtpHost = _configXml.GetElementsByTagName("SmtpHost");
				return smtpHost.Count > 0 
					? smtpHost[0].InnerText 
					: "";
			}
		}

		public bool SmtpEnabled => HasLoadedConfig && SmtpHost.Length > 0 && SmtpPort > -1;

		public int SmtpPort
		{
			get
			{
				if (!HasLoadedConfig)
					return -1;
				var smtpPort = _configXml.GetElementsByTagName("SmtpPort");
				if (smtpPort.Count > 0)
				{
					int port;
					if (int.TryParse(smtpPort[0].InnerText, out port))
						return port;
				}
				return -1;
			}
		}
		public bool SmtpUseSsl
		{
			get
			{
				if (!HasLoadedConfig)
					return true;
				var smtpUseSssl = _configXml.GetElementsByTagName("SmtpUseSsl");
				if (smtpUseSssl.Count > 0)
					return Convert.ToBoolean(smtpUseSssl[0].InnerText);
				return false;
			}
		}
		public string SmtpUser
		{
			get
			{
				if (!HasLoadedConfig)
					return "";
				var smtpUser = _configXml.GetElementsByTagName("SmtpUser");
				return smtpUser.Count > 0
					? smtpUser[0].InnerText
					: "";
			}
		}
		public string SmtpPassword
		{
			get
			{
				if (!HasLoadedConfig)
					return "";
				var smtpPassword = _configXml.GetElementsByTagName("SmtpPassword");
				return smtpPassword.Count > 0
					? smtpPassword[0].InnerText
					: "";
			}
		}

		public bool SmtpUseRelay
		{
			get
			{
				if (!HasLoadedConfig)
					return true;
				var smtpUseRelay = _configXml.GetElementsByTagName("SmtpUseRelay");
				if (smtpUseRelay.Count > 0)
					return Convert.ToBoolean(smtpUseRelay[0].InnerText);
				return false;
			}
		}

		public string ContentType
		{
			get
			{
				if (!HasLoadedConfig)
					return "";
				var contentType = _configXml.GetElementsByTagName("ContentType");
				return contentType.Count > 0 ? contentType[0].InnerText : "";
			}
		}

		public string EncodingName
		{
			get
			{
				if (!HasLoadedConfig)
					return "";
				var contentType = _configXml.GetElementsByTagName("EncodingName");
				return contentType.Count > 0 ? contentType[0].InnerText : "";
			}
		}
	}
}