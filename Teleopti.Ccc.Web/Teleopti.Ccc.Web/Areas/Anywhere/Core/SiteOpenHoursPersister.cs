using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class SiteOpenHoursPersister : ISiteOpenHoursPersister
	{
		private readonly ISiteRepository _siteRepository;

		public SiteOpenHoursPersister(ISiteRepository siteRepository)
		{
			_siteRepository = siteRepository;
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
					sitePersisted.OpenHours.Clear();
					foreach (SiteOpenHour openHour in site.OpenHours)
					{
						sitePersisted.OpenHours.Add(openHour.WeekDay, new TimePeriod(openHour.StartTime, openHour.EndTime));
					}
					updatedSitesCount++;
				}
			}
			return updatedSitesCount;
		}
	}
}