using System;
using System.Collections.Generic;
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
            HashSet<IPerson> uniquePersons = new HashSet<IPerson>();
            foreach (IScheduleDay scheduleDay in _scheduleDays)
            {
                uniquePersons.Add(scheduleDay.Person);
            }
            return new List<IPerson>(uniquePersons);
        }
    }
}