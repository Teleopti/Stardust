using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class DateAndTimeModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<Now>().As<INow>().SingleInstance();
		}
	}
}