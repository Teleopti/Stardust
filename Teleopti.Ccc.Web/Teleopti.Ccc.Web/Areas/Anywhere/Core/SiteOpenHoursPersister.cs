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
			int updatedSitesCount = 0;
			if (!sites.Any()) return updatedSitesCount;
			foreach (var site in sites)
			{
				var sitePersisted = _siteRepository.Get(site.Id);
				if (site.OpenHours != null)
				{
					foreach (var persistedOpenHour in sitePersisted.OpenHourCollection)
					{
						_siteOpenHourRepository.Remove(persistedOpenHour);
					}
					sitePersisted.ClearOpenHourCollection();

					foreach (SiteOpenHourViewModel openHour in site.OpenHours)
					{
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
			}
			return updatedSitesCount;
		}
	}
}