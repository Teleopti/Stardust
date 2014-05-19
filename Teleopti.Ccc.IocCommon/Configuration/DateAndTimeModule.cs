using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class DateAndTimeModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<MutableNow>().As<INow>().As<IMutateNow>().SingleInstance();
			builder.RegisterType<Time>().As<ITime>().SingleInstance();
			builder.RegisterType<TimeFormatter>().As<ITimeFormatter>().SingleInstance();
		}
	}
}