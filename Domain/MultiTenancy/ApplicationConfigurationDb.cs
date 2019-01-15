using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.MultiTenancy
{
	public class ApplicationConfigurationDb
	{
		public Dictionary<string, string> Server { get; set; }
		public Dictionary<string, string> Tenant { get; set; }
	}
}
