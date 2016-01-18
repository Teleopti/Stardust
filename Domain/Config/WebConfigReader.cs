using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;

namespace Teleopti.Ccc.Domain.Config
{
	public class WebConfigReader : IConfigReader
	{
		private readonly IDictionary<string, string> _appSettings;

		public WebConfigReader(Func<WebSettings> settings)
		{
			_appSettings = settings.Invoke().Settings;
		}

		public string AppConfig(string name)
		{
			string value;
			_appSettings.TryGetValue(name, out value);
			return value;
		}

		public string ConnectionString(string name)
		{
			var connectionStringSetting = ConfigurationManager.ConnectionStrings[name];
			return connectionStringSetting == null ? null : connectionStringSetting.ConnectionString;
		}

		public NameValueCollection AppSettings_DontUse
		{
			get { return ConfigurationManager.AppSettings; }
		}
	}
	
	public class WebSettings
	{
		public IDictionary<string, string> Settings { get; set; }
	}
}