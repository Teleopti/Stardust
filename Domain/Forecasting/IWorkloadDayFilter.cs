using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public interface IWorkloadDayFilter<T> where T : ITaskOwner
    {
        IList<T> FilterStatistics(IList<T> workloadDays, IEnumerable<DateOnlyPeriod> dateCollection);
    }
}