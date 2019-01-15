using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar
{
	public interface IBankHolidayCalendarSite : IAggregateRoot
	{
		ISite Site { get; set; }
		IBankHolidayCalendar Calendar { get; set; }
	}
}
