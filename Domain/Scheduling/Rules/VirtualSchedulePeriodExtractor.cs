using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
    public interface IVirtualSchedulePeriodExtractor
    {
        IEnumerable<IVirtualSchedulePeriod> CreateVirtualSchedulePeriodsFromScheduleDays(IEnumerable<IScheduleDay> scheduleDays);
    }

    public class VirtualSchedulePeriodExtractor : IVirtualSchedulePeriodExtractor
    {
        public IEnumerable<IVirtualSchedulePeriod> CreateVirtualSchedulePeriodsFromScheduleDays(IEnumerable<IScheduleDay> scheduleDays)
        {
            var periods = new HashSet<IVirtualSchedulePeriod>();

            foreach (IScheduleDay day in scheduleDays)
            {
                var startDate = day.DateOnlyAsPeriod.DateOnly;
                periods.Add(day.Person.VirtualSchedulePeriod(startDate));
            }

            return periods;
        }
    }
}