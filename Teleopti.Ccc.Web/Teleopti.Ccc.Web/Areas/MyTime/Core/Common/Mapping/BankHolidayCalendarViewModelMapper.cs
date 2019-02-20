using Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping
{
	public class BankHolidayCalendarViewModelMapper
	{
		public BankHolidayCalendarViewModel Map(IBankHolidayDate bankHolidayDate)
		{
			if (bankHolidayDate?.Calendar == null) return null;

			return new BankHolidayCalendarViewModel
			{
				CalendarId = bankHolidayDate.Calendar.Id.GetValueOrDefault(),
				CalendarName = bankHolidayDate.Calendar.Name,
				DateDescription = bankHolidayDate.Description
			};
		}
	}
}