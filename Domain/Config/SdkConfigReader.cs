using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;

namespace Teleopti.Ccc.Domain.Config
{
	public class SdkConfigReader : IConfigReader
	{
		private readonly IDictionary<string, string> _appSettings;

		public SdkConfigReader(Func<WebSettings> settings)
		{
			_appSettings = settings.Invoke().Settings;
		}

		public string AppConfig(string name)
		{
			string value;
			_appSettings.TryGetValue(name, out value);
			return value;
		}

		public IDictionary<string, string> ApplicationConfigs()
		{
			return _appSettings;
		}

		public string ConnectionString(string name)
		{
			return ConfigurationManager.ConnectionStrings[name].ConnectionString;
		}

		public NameValueCollection AppSettings_DontUse
		{
			get { return ConfigurationManager.AppSettings; }
		}

	}
}