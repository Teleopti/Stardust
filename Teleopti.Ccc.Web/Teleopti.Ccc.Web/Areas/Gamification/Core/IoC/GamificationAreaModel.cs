using Autofac;
using Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.Gamification.Mapping;

namespace Teleopti.Ccc.Web.Areas.Gamification.core.IoC
{
	public class GamificationAreaModel : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<QualityInfoProvider>().As<IQualityInfoProvider>().SingleInstance();
			builder.RegisterType<GamificationSettingMapper>().As<IGamificationSettingMapper>().SingleInstance();
			builder.RegisterType<GamificationSettingPersister>().As<IGamificationSettingPersister>().SingleInstance();
			builder.RegisterType<GamificationSettingProvider>().As<IGamificationSettingProvider>().SingleInstance();
		}
	}
}