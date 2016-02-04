using Autofac;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class IntradayWebModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<FetchSkillInIntraday>().SingleInstance();
			builder.RegisterType<FetchSkillArea>().SingleInstance();
			builder.RegisterType<CreateSkillArea>().SingleInstance();
			builder.RegisterType<LoadSkillInIntradays>().As<ILoadAllSkillInIntradays>().SingleInstance();
		}
	}
}