using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.SystemSettingWeb;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Models;

namespace Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.Mapping
{
	public class BankHolidayModelMapper : IBankHolidayModelMapper
	{
		public IBankHolidayDate Map(BankHolidayDateForm date)
		{
			var _date = new BankHolidayDate() { Date = date.Date, Description = date.Description };
			if (date.Id.HasValue)
				_date.SetId(date.Id.Value);
			if (date.IsDeleted)
				_date.SetDeleted();
			return _date;
		}

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

		public BankHolidayCalendarViewModel MapModelChanged(IBankHolidayCalendar calendar, BankHolidayCalendarForm input)
		{
			var dates = new List<BankHolidayDateViewModel>();

			var _dates = calendar.Dates.ToList();

			input?.Dates?.ToList().ForEach(
				d =>
				{
					if (d.Id.HasValue)
					{
						dates.Add(new BankHolidayDateViewModel
						{
							Id = d.Id.Value,
							Description = d.Description,
							IsDeleted = d.IsDeleted,
							Date = d.Date
						});
					}
					else
					{
						var dx = _dates.Find(_d => _d.Date == d.Date);
						dates.Add(new BankHolidayDateViewModel
						{
							Id = dx.Id.Value,
							Description = dx.Description,
							IsDeleted = dx.IsDeleted,
							Date = dx.Date
						});
					}
				});

			return MapBankHolidayCalendar(calendar, dates);
		}

		public IEnumerable<BankHolidayCalendarViewModel> Map(IEnumerable<IBankHolidayCalendar> calendars)
		{
			var models = new List<BankHolidayCalendarViewModel>();
			calendars?.ToList().ForEach(c => models.Add(Map(c)));
			return models.OrderBy(m => m.Name);
		}
	}
}