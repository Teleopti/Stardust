using System.Configuration;

namespace Teleopti.Ccc.WinCode.MultipleConfig
{
	public interface IConfigOverrider
	{
		AppSettingsSection AppSettings();
	}
}