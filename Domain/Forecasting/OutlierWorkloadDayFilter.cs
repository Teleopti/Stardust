using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class OutlierWorkloadDayFilter<T> : IWorkloadDayFilter<T> where T : ITaskOwner
    {
        private readonly IWorkload _workload;
        private readonly IOutlierRepository _outlierRepository;

        public OutlierWorkloadDayFilter(IWorkload workload, IOutlierRepository outlierRepository)
        {
            _workload = workload;
            _outlierRepository = outlierRepository;
        }

        public IList<T> FilterStatistics(IList<T> workloadDays, IEnumerable<DateOnlyPeriod> dateCollection)
        {
            //Load outliers
            IList<IOutlier> outliers = _outlierRepository.FindByWorkload(_workload);

            DateOnlyPeriod giantPeriod = new DateOnlyPeriod(dateCollection.Min(d => d.StartDate), dateCollection.Max(d => d.EndDate));

            //Exclude outliers from selection
            return
                StatisticHelper.ExcludeOutliersFromStatistics(giantPeriod, outliers, workloadDays);
        }
    }
}