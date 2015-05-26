using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class ConfigurationManagerWrapper : IConfigurationWrapper
	{
		public IDictionary<string, string> AppSettings
		{
			get { return StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings; }
		}
	}
}