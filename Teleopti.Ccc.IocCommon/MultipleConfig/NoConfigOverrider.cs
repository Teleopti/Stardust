using System.Configuration;

namespace Teleopti.Ccc.IocCommon.MultipleConfig
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