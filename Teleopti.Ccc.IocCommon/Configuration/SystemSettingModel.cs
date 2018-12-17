using Autofac;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class SystemSettingModel: Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<BankHolidayCalendarRepository>().As<IBankHolidayCalendarRepository>().SingleInstance();
		}
	}
}
