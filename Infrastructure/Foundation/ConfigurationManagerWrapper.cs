using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure;

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