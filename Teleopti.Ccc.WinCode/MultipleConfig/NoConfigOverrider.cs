using System.Configuration;

namespace Teleopti.Ccc.WinCode.MultipleConfig
{
	public class NoConfigOverrider : IConfigOverrider
	{
		private static readonly AppSettingsSection appSettingsSection = new AppSettingsSection();

		public AppSettingsSection AppSettings()
		{
			return appSettingsSection;
		}
	}
}