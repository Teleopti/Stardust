using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Models;

namespace Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.DataProvider
{
	public class SiteBankHolidayCalendarsProvider : ISiteBankHolidayCalendarsProvider
	{
		private readonly ISiteBankHolidayCalendarRepository _siteBankHolidayCalendarRepository;

		public SiteBankHolidayCalendarsProvider(ISiteBankHolidayCalendarRepository siteBankHolidayCalendarRepository)
		{
			_siteBankHolidayCalendarRepository = siteBankHolidayCalendarRepository;
		}

		public IEnumerable<SiteBankHolidayCalendarsViewModel> GetAllSettings()
		{
			var allSettings = _siteBankHolidayCalendarRepository.FindAllSiteBankHolidayCalendarsSortedBySite();

			return allSettings.Select(siteBankHolidayCalendar => new SiteBankHolidayCalendarsViewModel
				{
					Site = siteBankHolidayCalendar.Site.Id.GetValueOrDefault(),
					Calendars = siteBankHolidayCalendar.BankHolidayCalendarsForSite.Select(calendar => new BankHolidayCalendarInfoViewModel
						{
							Id = calendar.Id.GetValueOrDefault(),
							Name = calendar.Name
						})
				});
		}
	}
}