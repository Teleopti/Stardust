using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class PersonListExtractorFromScheduleParts : IPersonListExtractorFromScheduleParts
    {
		public IList<IPerson> ExtractPersons(IEnumerable<IScheduleDay> scheduleDays)
        {
	        return scheduleDays.Select(s => s.Person).Distinct().ToList();
        }
    }
}