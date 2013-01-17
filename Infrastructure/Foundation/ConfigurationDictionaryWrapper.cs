using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class ConfigurationDictionaryWrapper : IConfigurationWrapper
	{
		public ConfigurationDictionaryWrapper(IDictionary<string,string> settings)
		{
			AppSettings = settings;
		}

		public IDictionary<string, string> AppSettings { get; private set; } 
	}
}