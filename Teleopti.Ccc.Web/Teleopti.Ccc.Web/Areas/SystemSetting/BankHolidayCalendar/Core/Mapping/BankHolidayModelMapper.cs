using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.SystemSettingWeb;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Models;

namespace Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.Mapping
{
	public class BankHolidayModelMapper : IBankHolidayModelMapper
	{
		public BankHolidayCalendarViewModel Map(IBankHolidayCalendar calendar)
		{

			var dates = new List<BankHolidayDateViewModel>();

			calendar?.Dates?.ToList().ForEach(d =>
			{
				if (d.Id.HasValue && !d.IsDeleted)
					dates.Add(new BankHolidayDateViewModel { Date = d.Date, Description = d.Description, Id = d.Id.Value });
			});

			return MapBankHolidayCalendar(calendar, dates);
		}

		private BankHolidayCalendarViewModel MapBankHolidayCalendar(IBankHolidayCalendar calendar, List<BankHolidayDateViewModel> dates)
		{
			if (!calendar.Id.HasValue)
				return null;
			var model = new BankHolidayCalendarViewModel();
			model.Id = calendar.Id.Value;
			model.Name = calendar.Name;

			var years = new List<BankHolidayYearViewModel>();

			var query = from d in dates
						orderby d.Date
						group d by d.Date.Year into g
						orderby g.Key
						select g;

			foreach (IGrouping<int, BankHolidayDateViewModel> gx in query)
			{
				years.Add(new BankHolidayYearViewModel { Dates = gx.ToList() });
			}

			model.Years = years;

			return model;
		}

		public IEnumerable<BankHolidayCalendarViewModel> Map(IEnumerable<IBankHolidayCalendar> calendars)
		{
			var models = new List<BankHolidayCalendarViewModel>();
			calendars?.ToList().ForEach(c => models.Add(Map(c)));
			return models.OrderBy(m => m.Name);
		}
	}
}