using System.Configuration;
using Teleopti.Ccc.IocCommon.MultipleConfig;

namespace Teleopti.Ccc.IocCommonTest.MultipleConfig
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