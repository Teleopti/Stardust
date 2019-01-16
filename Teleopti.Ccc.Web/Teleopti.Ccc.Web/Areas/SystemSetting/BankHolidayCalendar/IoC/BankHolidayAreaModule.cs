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
			builder.RegisterType<BankHolidayCalendarPersister>().As<IBankHolidayCalendarPersister>().SingleInstance();
			builder.RegisterType<BankHolidayCalendarProvider>().As<IBankHolidayCalendarProvider>().SingleInstance();
			builder.RegisterType<BankHolidayModelMapper>().As<IBankHolidayModelMapper>().SingleInstance();
			builder.RegisterType<BankHolidayCalendarSiteProvider>().As<IBankHolidayCalendarSiteProvider>().SingleInstance();
			builder.RegisterType<BankHolidayCalendarSitePersister>().As<IBankHolidayCalendarSitePersister>().SingleInstance();
		}
	}
}