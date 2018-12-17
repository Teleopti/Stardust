using Autofac;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.DataProvider;

namespace Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.IoC
{
	public class BankHolidayAreaModule: Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<BankHolidayCalendarPersister>().As<IBankHolidayCalendarPersister>();
		}
	}
}