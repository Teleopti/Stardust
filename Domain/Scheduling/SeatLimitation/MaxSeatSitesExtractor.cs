using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
    public interface IMaxSeatSitesExtractor
    {
        HashSet<ISite> MaxSeatSites(DateOnlyPeriod requestedPeriod);
    }

    public class MaxSeatSitesExtractor : IMaxSeatSitesExtractor
    {
        private readonly IEnumerable<IPerson> _allPermittedPersons;
        
        public MaxSeatSitesExtractor(IEnumerable<IPerson> allPermittedPersons)
        {
            _allPermittedPersons = allPermittedPersons;
        }

        public HashSet<ISite> MaxSeatSites(DateOnlyPeriod requestedPeriod)
        {
            var sitesWithMaxSeats = new HashSet<ISite>();
            
            foreach (var person in _allPermittedPersons)
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