using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.SystemSettingWeb;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Models;

namespace Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.Mapping
{
	public class BankHolidayModelMapper : IBankHolidayModelMapper
	{
		public BankHolidayCalendarViewModel Map(IBankHolidayCalendar calendar, IEnumerable<IBankHolidayDate> dates)
		{
			if (!calendar.Id.HasValue)
				return null;

			var model = new BankHolidayCalendarViewModel {Id = calendar.Id.Value, Name = calendar.Name};

			var viewModels = (dates ?? Enumerable.Empty<IBankHolidayDate>()).Where(d => d.Id.HasValue && !d.IsDeleted).Select(d => new BankHolidayDateViewModel { Date = d.Date, Description = d.Description, Id = d.Id.Value });
			var years = from d in viewModels
						orderby d.Date
						group d by d.Date.Year into g
						orderby g.Key
						select g;
			
			model.Years = years.Select(year => new BankHolidayYearViewModel { Dates = year.ToList() }).ToArray();

			return model;
		}

		public IEnumerable<BankHolidayCalendarViewModel> Map(IEnumerable<IBankHolidayCalendar> calendars, IEnumerable<IBankHolidayDate> dates)
		{
			var datesByCalendar = dates.ToLookup(d => d.Calendar);

			var models = (calendars ?? Enumerable.Empty<IBankHolidayCalendar>()).Select(c => Map(c, datesByCalendar[c]));
			return models.OrderBy(m => m.Name);
		}
	}
}