using System.Collections.Generic;
using System.Configuration;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class ConfigurationManagerWrapper : IConfigurationWrapper
	{
		public ConfigurationManagerWrapper()
		{
			AppSettings = ConfigurationManager.AppSettings.ToDictionary();
		}

		public IDictionary<string, string> AppSettings { get; private set; } 
	}
}