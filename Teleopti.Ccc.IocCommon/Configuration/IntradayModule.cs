using Autofac;
using Teleopti.Ccc.Domain.Intraday;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class IntradayModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<FetchSkillInIntraday>().SingleInstance();
		}
	}
}