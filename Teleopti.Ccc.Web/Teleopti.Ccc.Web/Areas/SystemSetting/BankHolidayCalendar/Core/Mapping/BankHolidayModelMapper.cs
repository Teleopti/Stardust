using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
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
			var model = new BankHolidayCalendarViewModel();
			model.Id = calendar.Id.Value;
			model.Name = calendar.Name;

			var years = new List<BankHolidayYearViewModel>();

			var _dates = new List<BankHolidayDateViewModel>();

			dates?.ToList().ForEach(d =>
			{
				if (d.Id.HasValue && !d.IsDeleted)
					_dates.Add(new BankHolidayDateViewModel { Date = d.Date.ToDateOnly(), Description = d.Description, Id = d.Id.Value });
			});

			var query = from d in _dates
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

		public IEnumerable<BankHolidayCalendarViewModel> Map(IEnumerable<IBankHolidayCalendar> calendars, IEnumerable<IBankHolidayDate> dates)
		{
			var models = new List<BankHolidayCalendarViewModel>();
			calendars?.ToList().ForEach(c =>
			{
				var _dates = dates.Where(d => d.Calendar.Id.Value == c.Id.Value);
				var model = Map(c, _dates);
				models.Add(model);
			});
			return models.OrderBy(m => m.Name);
		}
	}
}