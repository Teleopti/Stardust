using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Models;

namespace Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.DataProvider
{
	public class SiteBankHolidayCalendarsProvider : ISiteBankHolidayCalendarsProvider
	{
		private readonly ISiteBankHolidayCalendarRepository _siteBankHolidayCalendarRepository;
		private readonly ISiteRepository _siteRepository;
		private readonly IBankHolidayCalendarRepository _bankHolidayCalendarRepository;

		public SiteBankHolidayCalendarsProvider(ISiteBankHolidayCalendarRepository siteBankHolidayCalendarRepository, 
			ISiteRepository siteRepository, IBankHolidayCalendarRepository bankHolidayCalendarRepository)
		{
			_siteBankHolidayCalendarRepository = siteBankHolidayCalendarRepository;
			_siteRepository = siteRepository;
			_bankHolidayCalendarRepository = bankHolidayCalendarRepository;
		}

		public IEnumerable<SiteBankHolidayCalendarsViewModel> GetAllSettings()
		{
			var allSettings = _siteBankHolidayCalendarRepository.FindAllSiteBankHolidayCalendarsSortedBySite();

			return allSettings.Select(siteBankHolidayCalendar => new SiteBankHolidayCalendarsViewModel
				{
					Site = siteBankHolidayCalendar.Site.Id.GetValueOrDefault(),
					Calendars = siteBankHolidayCalendar.BankHolidayCalendarsForSite.Select(calendar => calendar.Id.GetValueOrDefault())
				});
		}

		public void UpdateCalendarsForSites(IEnumerable<SiteBankHolidayCalendarsViewModel> input)
		{
			var allCurrentSettings = _siteBankHolidayCalendarRepository.FindAllSiteBankHolidayCalendarsSortedBySite().ToList();
			foreach (var siteBankHolidayCalendarsViewModel in input)
			{
				var existingSetting = allCurrentSettings.Find(x => x.Site.Id == siteBankHolidayCalendarsViewModel.Site);
				var calendars = _bankHolidayCalendarRepository.FindBankHolidayCalendars(siteBankHolidayCalendarsViewModel.Calendars);

				if (!calendars.Any())
				{
					_siteBankHolidayCalendarRepository.Remove(existingSetting);
					continue;
				}

				addOrUpdateCalendars(existingSetting, calendars, siteBankHolidayCalendarsViewModel.Site);
			}
		}

		public IEnumerable<Guid> GetSitesByAssignedCalendar(Guid calendarId)
		{
			var allMetchedResults = _siteBankHolidayCalendarRepository.FindSiteBankHolidayCalendars(calendarId);
			return allMetchedResults.Select(x => x.Site.Id.GetValueOrDefault());
		}

		private void addOrUpdateCalendars(ISiteBankHolidayCalendar existingSetting, ICollection<IBankHolidayCalendar> calendars, Guid newSettingSiteId)
		{
			if (existingSetting == null)
			{
				_siteBankHolidayCalendarRepository.Add(new SiteBankHolidayCalendar
				{
					Site = _siteRepository.Get(newSettingSiteId),
					BankHolidayCalendarsForSite = calendars
				});
			}
			else
			{
				var existingNotUpdated = existingSetting.BankHolidayCalendarsForSite.Except(calendars);
				var updatedNotExisting = calendars.Except(existingSetting.BankHolidayCalendarsForSite);
				if(!existingNotUpdated.Any() && !updatedNotExisting.Any()) return;
				
				existingSetting.UpdateBankHolidayCalendarsForSite(calendars);
			}
		}
	}
}