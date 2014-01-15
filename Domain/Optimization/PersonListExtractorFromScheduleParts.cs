using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class PersonListExtractorFromScheduleParts : IPersonExtractor
    {
        private readonly IEnumerable<IScheduleDay> _scheduleDays;

        public PersonListExtractorFromScheduleParts(IEnumerable<IScheduleDay> scheduleDays)
        {
            _scheduleDays = scheduleDays;
        }

        public IEnumerable<IPerson> ExtractPersons()
        {
	        return _scheduleDays.Select(s => s.Person).Distinct().ToList();
        }
    }
}