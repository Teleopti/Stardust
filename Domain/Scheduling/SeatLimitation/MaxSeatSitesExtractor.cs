using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public class MaxSeatSitesExtractor
    {
		public ISite[] MaxSeatSites(DateOnlyPeriod requestedPeriod, IEnumerable<IPerson> personsInOrganization)
		{
			return personsInOrganization.SelectMany(p => p.PersonPeriods(requestedPeriod))
				.Select(pp => pp.Team?.Site)
				.Distinct()
				.Where(s => s?.MaxSeats != null)
				.ToArray();
        }
    }
}