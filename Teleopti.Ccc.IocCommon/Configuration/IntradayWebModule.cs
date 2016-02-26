using Autofac;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class IntradayWebModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<FetchSkillInIntraday>().SingleInstance();
			builder.RegisterType<FetchSkillArea>().SingleInstance();
			builder.RegisterType<CreateSkillArea>().SingleInstance();
			builder.RegisterType<DeleteSkillArea>().SingleInstance();
			builder.RegisterType<LoadSkillInIntradays>().As<ILoadAllSkillInIntradays>().SingleInstance();
			builder.RegisterType<IntradayMonitorDataLoader>().As<IIntradayMonitorDataLoader>().SingleInstance();
			builder.RegisterType<MonitorSkillAreaProvider>().SingleInstance();
		}
	}
}