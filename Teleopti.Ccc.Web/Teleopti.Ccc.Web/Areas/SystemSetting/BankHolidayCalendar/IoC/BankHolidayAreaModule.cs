using Autofac;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.Mapping;

namespace Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.IoC
{
	public class BankHolidayAreaModule: Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<BankHolidayCalendarPersister>().As<IBankHolidayCalendarPersister>();
			
			builder.RegisterType<BankHolidayDateRepository>().As<IBankHolidayDateRepository>();

			builder.RegisterType<BankHolidayCalendarProvider>().As<IBankHolidayCalendarProvider>();

			builder.RegisterType<BankHolidayModelMapper>().As<IBankHolidayModelMapper>();

		}
	}
}