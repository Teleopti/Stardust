using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class ConfigurationManagerWrapper : IConfigurationWrapper
	{
		public ConfigurationManagerWrapper()
		{
			AppSettings = new Dictionary<string, string>();
			ConfigurationManager.AppSettings.AllKeys.ToList().ForEach(name => AppSettings.Add(name, ConfigurationManager.AppSettings[name]));
			var published = (NameValueCollection)ConfigurationManager.GetSection("teleopti/publishedSettings");

			if (published != null)
			{
				foreach (string item in published)
				{
					AppSettings.Add(item, published.Get(item));
				}
			}
		}

		public IDictionary<string, string> AppSettings { get; private set; } 
	}
}