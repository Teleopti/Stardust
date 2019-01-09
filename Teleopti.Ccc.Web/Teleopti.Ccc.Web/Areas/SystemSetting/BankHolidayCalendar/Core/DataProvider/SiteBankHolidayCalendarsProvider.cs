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
					Calendars = siteBankHolidayCalendar.BankHolidayCalendarsForSite.Select(calendar => new BankHolidayCalendarInfoViewModel
						{
							Id = calendar.Id.GetValueOrDefault(),
							Name = calendar.Name
						})
				});
		}

		public void UpdateCalendarsForSites(IEnumerable<SiteBankHolidayCalendarsViewModel> input)
		{
			var allCurrentSettings = _siteBankHolidayCalendarRepository.FindAllSiteBankHolidayCalendarsSortedBySite().ToList();
			foreach (var siteBankHolidayCalendarsViewModel in input)
			{
				var existingSetting = allCurrentSettings.Find(x => x.Site.Id == siteBankHolidayCalendarsViewModel.Site);
				var calendarIds = siteBankHolidayCalendarsViewModel.Calendars.Select(calendar => calendar.Id);
				var calendars = _bankHolidayCalendarRepository.FindBankHolidayCalendars(calendarIds);

				if (!calendars.Any())
				{
					_siteBankHolidayCalendarRepository.Remove(existingSetting);
					continue;
				}

				addOrUpdateCalendars(existingSetting, calendars, siteBankHolidayCalendarsViewModel.Site);
			}
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
				existingSetting.UpdateBankHolidayCalendarsForSite(calendars);
			}
		}
	}
}