using System;
using System.Collections.Generic;
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
			return connectionStringSetting?.ConnectionString;
		}
		
		public IDictionary<string, string> WebSettings_DontUse => _appSettings;
	}
	
	public class WebSettings
	{
		public IDictionary<string, string> Settings { get; set; }
	}
}