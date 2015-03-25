using System.Configuration;

namespace Teleopti.Ccc.IocCommon.MultipleConfig
{
	public interface IConfigOverrider
	{
		string AppSetting(string key);
	}
}