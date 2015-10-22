using Autofac;

namespace Teleopti.Ccc.Web.Areas.Intraday
{
	public class IntradayAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SkillTasksDetailProvider>().As<ISkillTasksDetailProvider>().SingleInstance();
			
		}
	}
}