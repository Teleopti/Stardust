using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
	public interface IMaxSeatSitesExtractor
    {
		HashSet<ISite> MaxSeatSites(DateOnlyPeriod requestedPeriod, IEnumerable<IPerson> personsInOrganization);
    }

	[RemoveMeWithToggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
	public class MaxSeatSitesExtractor : IMaxSeatSitesExtractor
    {
		public HashSet<ISite> MaxSeatSites(DateOnlyPeriod requestedPeriod, IEnumerable<IPerson> personsInOrganization)
        {
            var sitesWithMaxSeats = new HashSet<ISite>();

			foreach (var person in personsInOrganization)
            {
                for (DateOnly currentDate = requestedPeriod.StartDate; currentDate <= requestedPeriod.EndDate; currentDate = currentDate.AddDays(1))
                {
                    IPersonPeriod personPeriod = person.Period(currentDate);
                    if (personPeriod == null)
                        continue;

                    var site = personPeriod.Team.Site;
                    if (site.MaxSeats.HasValue)
                        sitesWithMaxSeats.Add(site);

                    currentDate = personPeriod.EndDate().AddDays(1);
                }
            }
            return sitesWithMaxSeats;
        }
    }
}