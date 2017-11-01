using Autofac;
using Teleopti.Ccc.Web.Areas.Gamification.core.DataProvider;

namespace Teleopti.Ccc.Web.Areas.Gamification.core.IoC
{
	public class GamificationAreaModel : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<GamificationSettingPersister>().As<IGamificationSettingPersister>().SingleInstance();
		}
	}
}