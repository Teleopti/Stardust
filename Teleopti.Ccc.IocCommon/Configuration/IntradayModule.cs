using Autofac;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class IntradayModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<FetchSkillInIntraday>().SingleInstance();
			builder.RegisterType<FetchSkillArea>().SingleInstance();
			builder.RegisterType<LoadAllSkillInIntradays>().As<ILoadAllSkillInIntradays>().SingleInstance();
		}
	}
}