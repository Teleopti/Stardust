using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class NoneWorkloadDayFilter<T> : IWorkloadDayFilter<T> where T : ITaskOwner
    {
        public IList<T> FilterStatistics(IList<T> workloadDays, IEnumerable<DateOnlyPeriod> dateCollection)
        {
            return workloadDays;
        }
    }
}