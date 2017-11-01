using Autofac;
using Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider;

namespace Teleopti.Ccc.Web.Areas.Gamification.core.IoC
{
	public class GamificationAreaModel : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<GamificationSettingPersister>().As<IGamificationSettingPersister>().SingleInstance();
			builder.RegisterType<GamificationSettingProvider>().As<IGamificationSettingProvider>().SingleInstance();
		}
	}
}