using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.IocCommon
{
	public interface IIocConfiguration
	{
		//string AppSetting(string setting);
		bool Toggle(Toggles toggle);
		IocArgs Args();
		IocCache Cache();
	}
}