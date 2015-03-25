using System.Configuration;

namespace Teleopti.Ccc.IocCommon.MultipleConfig
{
	public interface IConfigOverrider
	{
		AppSettingsSection AppSettings();
	}
}