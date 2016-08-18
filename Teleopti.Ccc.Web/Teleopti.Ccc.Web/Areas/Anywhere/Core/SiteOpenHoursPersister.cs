using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
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
				foreach (var persistedOpenHour in sitePersisted.OpenHourCollection)
				{
					_siteOpenHourRepository.Remove(persistedOpenHour);
				}
				sitePersisted.ClearOpenHourCollection();

				foreach (var dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
				{
					var openHour = site.OpenHours.FirstOrDefault(o => o.WeekDay == (DayOfWeek) dayOfWeek)
								   ?? new SiteOpenHourViewModel {WeekDay = (DayOfWeek) dayOfWeek, IsClosed = true};
					var siteOpenHour = new SiteOpenHour()
					{
						TimePeriod = new TimePeriod(openHour.StartTime, openHour.EndTime),
						WeekDay = openHour.WeekDay,
						IsClosed = openHour.IsClosed
					};
					if (sitePersisted.AddOpenHour(siteOpenHour))
						_siteOpenHourRepository.Add(siteOpenHour);
				}
				updatedSitesCount++;
			}
			return updatedSitesCount;
		}
	}
}