using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class DateAndTimeModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<MutableNow>().As<INow>().As<IMutateNow>().SingleInstance();
			builder.RegisterType<UserNow>().As<IUserNow>().SingleInstance();
			builder.RegisterType<Time>().As<ITime>().SingleInstance();
			builder.RegisterType<TimeFormatter>().SingleInstance();
		}
	}
}