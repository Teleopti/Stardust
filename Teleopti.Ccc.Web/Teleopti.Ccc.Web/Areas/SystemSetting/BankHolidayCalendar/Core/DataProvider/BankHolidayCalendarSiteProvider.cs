using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Models;

namespace Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.DataProvider
{
	public class BankHolidayCalendarSiteProvider : IBankHolidayCalendarSiteProvider
	{
		private readonly IBankHolidayCalendarSiteRepository _bankHolidayCalendarSiteRepository;

		public BankHolidayCalendarSiteProvider(IBankHolidayCalendarSiteRepository bankHolidayCalendarSiteRepository)
		{
			_bankHolidayCalendarSiteRepository = bankHolidayCalendarSiteRepository;
		}

		public IEnumerable<SiteBankHolidayCalendarsViewModel> GetAllSettings()
		{
			var settings = _bankHolidayCalendarSiteRepository.LoadAll();
			var result = settings.GroupBy(s=>s.Site.Id.Value).Select(g => new SiteBankHolidayCalendarsViewModel { Site = g.Key, Calendars = g.Select(x => x.Calendar.Id.Value)});
			return result;
		}

		public IEnumerable<Guid> GetSitesByAssignedCalendar(Guid calendarId)
		{
			return _bankHolidayCalendarSiteRepository.FindSitesByCalendar(calendarId);
		}
	}
}