using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.IocCommon
{
	public interface IIocConfiguration
	{
		bool Toggle(Toggles toggle);
		IocArgs Args();
		IocCache Cache();
	}
}