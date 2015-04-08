using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Extracts a unique schedule part extractor list from a schedule part list.
    /// </summary>
    public class UniqueSchedulePartExtractor : IUniqueSchedulePartExtractor
    {
        public IEnumerable<ISchedulePartExtractor> ExtractUniqueScheduleParts(IEnumerable<IScheduleDay> schedulePartList)
        {
            var uniqueExtractorList = new HashSet<ISchedulePartExtractor>();
            foreach (var schedulePart in schedulePartList)
            {
                ISchedulePartExtractor extractor = new SchedulePartExtractor(schedulePart);
                if(extractor.SchedulePeriod.IsValid)
                    uniqueExtractorList.Add(extractor);
            }

            return uniqueExtractorList;
        }
    }
}
