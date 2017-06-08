using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Teleopti.Ccc.SmartClientPortal.Shell.ConfigurationSections
{
	public class ServerInstallations : ConfigurationSection
	{
		[ConfigurationProperty("installations")]
		public InstallationCollection Installations => (InstallationCollection) this["installations"];

		public static IDictionary<string, IDictionary<string, string>> FetchServerInstallations()
		{
			var serverInstallations = ConfigurationManager.GetSection("serverInstallations");
			if(serverInstallations==null)
				return new Dictionary<string, IDictionary<string, string>>();
			var installations = ((ServerInstallations)serverInstallations).Installations;
			var appsettingOverrides = new Dictionary<string, IDictionary<string, string>>();

			foreach (var serverInstallation in installations)
			{
				appsettingOverrides[serverInstallation.Name] = serverInstallation.Overrides.ToDictionary(k => k.Key, v => v.Value);
			}
			return appsettingOverrides;
		}
	}
}