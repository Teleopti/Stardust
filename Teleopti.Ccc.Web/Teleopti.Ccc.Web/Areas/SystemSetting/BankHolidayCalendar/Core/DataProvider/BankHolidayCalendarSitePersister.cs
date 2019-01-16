using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Models;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.DataProvider
{

	public class BankHolidayCalendarSitePersister : IBankHolidayCalendarSitePersister
	{
		private readonly IBankHolidayCalendarSiteRepository _bankHolidayCalendarSiteRepository;
		private readonly ISiteRepository _siteRepository;
		private readonly IBankHolidayCalendarRepository _bankHolidayCalendarRepository;
		private static readonly ILog logger = LogManager.GetLogger(typeof(BankHolidayCalendarPersister));

		public BankHolidayCalendarSitePersister(IBankHolidayCalendarSiteRepository bankHolidayCalendarSiteRepository, ISiteRepository siteRepository, IBankHolidayCalendarRepository bankHolidayCalendarRepository)
		{
			_bankHolidayCalendarSiteRepository = bankHolidayCalendarSiteRepository;
			_siteRepository = siteRepository;
			_bankHolidayCalendarRepository = bankHolidayCalendarRepository;
		}

		private void addCalendarsForSites(SiteBankHolidayCalendarsViewModel model, ISite site = null)
		{
			var _site = site ?? _siteRepository.Get(model.Site);
			var calendars = _bankHolidayCalendarRepository.FindBankHolidayCalendars(model.Calendars);
			calendars?.ToList().ForEach(c => _bankHolidayCalendarSiteRepository.Add(new BankHolidayCalendarSite { Site = _site, Calendar = c }));
		}

		private void removeCalendarsForSites(IEnumerable<IBankHolidayCalendarSite> settings)
		{
			settings?.ToList().ForEach(s => _bankHolidayCalendarSiteRepository.Remove(s));
		}

		private void updateCalendarsForSites(IEnumerable<IBankHolidayCalendarSite> settings, SiteBankHolidayCalendarsViewModel model)
		{
			var site = settings.FirstOrDefault().Site;
			var currentCalendars = settings.Select(s => s.Calendar.Id.Value);

			var deleteCalendars = currentCalendars.Except(model.Calendars);
			var deleteSettings = settings.Where(s => deleteCalendars.Contains(s.Calendar.Id.Value));
			removeCalendarsForSites(deleteSettings);

			var addCalendars = model.Calendars.Except(currentCalendars);
			var addSettings = new SiteBankHolidayCalendarsViewModel { Site = model.Site, Calendars = addCalendars };
			addCalendarsForSites(addSettings, site);
		}

		public bool UpdateCalendarsForSites(IEnumerable<SiteBankHolidayCalendarsViewModel> input)
		{
			var result = true;
			try
			{
				var settings = _bankHolidayCalendarSiteRepository.LoadAll();

				input?.ToList().ForEach(m =>
				{
					var settingx = settings.Where(s=>s.Site.Id.Value==m.Site);

					if (!settingx.Any())
					{
						addCalendarsForSites(m);
					}
					else if (m.Calendars == null || m.Calendars.Count() == 0)
					{
						removeCalendarsForSites(settingx);
					}
					else
					{
						updateCalendarsForSites(settingx, m);
					}

				});
			}
			catch (Exception ex)
			{
				logger.Error("Update Clendars For Sites failed.", ex);
				result = false;
			}

			return result;
		}
	}
}