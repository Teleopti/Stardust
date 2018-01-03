using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.IocCommon
{
	public interface IIocConfiguration
	{
		void FillToggles();
		bool Toggle(Toggles toggle);
		IocArgs Args();
		IocCache Cache();
	}
}