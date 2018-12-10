using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours
{
	public class SiteOpenHoursPersister : ISiteOpenHoursPersister
	{
		private readonly ISiteRepository _siteRepository;
		private readonly ISiteOpenHourRepository _siteOpenHourRepository;

		public SiteOpenHoursPersister(ISiteRepository siteRepository, ISiteOpenHourRepository siteOpenHourRepository)
		{
			_siteRepository = siteRepository;
			_siteOpenHourRepository = siteOpenHourRepository;
		}

		public int Persist(IEnumerable<SiteViewModel> sites)
		{
			var updatedSitesCount = 0;
			if (!sites.Any())
				return updatedSitesCount;

			foreach (var site in sites)
			{
				if (site.OpenHours == null)
				{
					continue;
				}

				var sitePersisted = _siteRepository.Get(site.Id);

				clearOriginalOpenHours(sitePersisted);

				if (!site.OpenHours.Any())
					continue;

				if (site.OpenHours.All(openHour => openHour.IsClosed))
				{
					saveAsClosedForSite(site, sitePersisted);
				}
				else
				{
					foreach (var dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
					{
						var siteOpenHour = createSiteOpenHour((DayOfWeek)dayOfWeek, site);
						if (sitePersisted.AddOpenHour(siteOpenHour))
							_siteOpenHourRepository.Add(siteOpenHour);
					}
				}

				updatedSitesCount++;
			}
			return updatedSitesCount;
		}

		private void saveAsClosedForSite(SiteViewModel site, ISite sitePersisted)
		{
			var firstSiteOpenHourView = site.OpenHours.FirstOrDefault();
			var timePeriod = new TimePeriod(firstSiteOpenHourView.StartTime, firstSiteOpenHourView.EndTime);
			foreach (var dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
			{
				var siteOpenHour = new SiteOpenHour()
				{
					TimePeriod = timePeriod,
					WeekDay = (DayOfWeek)dayOfWeek,
					IsClosed = true
				};
				if (sitePersisted.AddOpenHour(siteOpenHour))
					_siteOpenHourRepository.Add(siteOpenHour);
			}
		}

		private void clearOriginalOpenHours(ISite sitePersisted)
		{
			foreach (var persistedOpenHour in sitePersisted.OpenHourCollection)
			{
				_siteOpenHourRepository.Remove(persistedOpenHour);
			}
			sitePersisted.ClearOpenHourCollection();
		}

		private SiteOpenHour createSiteOpenHour(DayOfWeek dayOfWeek, SiteViewModel site)
		{
			var openHourView = site.OpenHours.FirstOrDefault(o => o.WeekDay == dayOfWeek && !o.IsClosed);
			if (openHourView != null)
			{
				return new SiteOpenHour
				{
					TimePeriod = new TimePeriod(openHourView.StartTime, openHourView.EndTime),
					WeekDay = dayOfWeek,
					IsClosed = false
				};
			}

			return new SiteOpenHour
			{
				TimePeriod = new TimePeriod(),
				WeekDay = dayOfWeek,
				IsClosed = true
			};
		}
	}
}
