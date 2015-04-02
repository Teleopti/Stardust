using System.Collections.Generic;
using System.Configuration;

namespace Teleopti.Ccc.SmartClientPortal.Shell.ConfigurationSections
{
	public class ServerInstallations : ConfigurationSection
	{
		[ConfigurationProperty("installations")]
		public InstallationCollection Installations
		{
			get { return (InstallationCollection) this["installations"]; }
		}

		public static IDictionary<string, IDictionary<string, string>> FetchServerInstallations()
		{
			var serverInstallations = ((ServerInstallations)ConfigurationManager.GetSection("serverInstallations")).Installations;
			var appsettingOverrides = new Dictionary<string, IDictionary<string, string>>();

			foreach (var serverInstallation in serverInstallations)
			{
				var appsettings = new Dictionary<string, string>();
				foreach (var appSetting in serverInstallation.Overrides)
				{
					appsettings[appSetting.Key] = appSetting.Value;
				}
				appsettingOverrides[serverInstallation.Name] = appsettings;
			}
			return appsettingOverrides;
		}
	}
}