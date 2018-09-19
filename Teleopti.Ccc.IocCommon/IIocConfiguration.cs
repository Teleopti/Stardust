using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.IocCommon
{
	public interface IIocConfiguration
	{
		void FillToggles();
		bool Toggle(Toggles toggle);
		IocArgs Args();
		void AddToggleManagerToBuilder(ContainerBuilder builder);
	}
}