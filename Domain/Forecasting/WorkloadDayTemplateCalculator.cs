using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public interface IWorkloadDayTemplateCalculator
	{
		/// <summary>
		/// Loads the workload day templates.
		/// </summary>
		/// <param name="dateCollection">The date collection.</param>
		/// <param name="workload">The workload.</param>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-04-09
		/// </remarks>
		void LoadWorkloadDayTemplates(IList<DateOnlyPeriod> dateCollection, IWorkload workload);
	}

	public class WorkloadDayTemplateCalculator : IWorkloadDayTemplateCalculator
	{
        private readonly IStatisticHelper _statisticHelper;
        private readonly IOutlierRepository _outlierRepository;

        public WorkloadDayTemplateCalculator(IStatisticHelper statisticHelper, IOutlierRepository outlierRepository)
        {
            _statisticHelper = statisticHelper;
            _outlierRepository = outlierRepository;
        }

        /// <summary>
        /// Adds the workload day templates.
        /// </summary>
        /// <param name="dateCollection">The date collection.</param>
        /// <param name="workload">The workload.</param>
        /// <param name="dayIndex">Index of the day.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-27
        /// </remarks>
        public void RecalculateWorkloadDayTemplate(IList<DateOnlyPeriod> dateCollection, IWorkload workload, int dayIndex)
        {
            IList<IWorkloadDayBase> workloadDays = CollectStatistics(dateCollection, workload, new NoneWorkloadDayFilter<IWorkloadDayBase>());

            Statistic statistic = new Statistic(workload);
            statistic.CalculateCustomTemplateDay(workloadDays, dayIndex);
        }

        private IList<IWorkloadDayBase> CollectStatistics(IEnumerable<DateOnlyPeriod> dateCollection, IWorkload workload, IWorkloadDayFilter<IWorkloadDayBase> workloadDayFilter)
        {
            var workloadDays = new List<IWorkloadDayBase>();
            foreach (DateOnlyPeriod period in dateCollection)
            {
                workloadDays.AddRange(_statisticHelper.LoadStatisticData(period, workload));
            }

            //In case of double date periods in original selection
            workloadDays = workloadDays.Distinct().ToList();

            return workloadDayFilter.FilterStatistics(workloadDays, dateCollection);
        }

        /// <summary>
        /// Loads the workload day templates.
        /// </summary>
        /// <param name="dateCollection">The date collection.</param>
        /// <param name="workload">The workload.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-09
        /// </remarks>
        public void LoadWorkloadDayTemplates(IList<DateOnlyPeriod> dateCollection, IWorkload workload)
        {
            IList<IWorkloadDayBase> workloadDays = CollectStatistics(dateCollection, workload, new OutlierWorkloadDayFilter<IWorkloadDayBase>(workload, _outlierRepository));

            Statistic statistic = new Statistic(workload);
            statistic.CalculateTemplateDays(workloadDays);
        }

        public void RecalculateWorkloadDayTemplate(IList<DateOnlyPeriod> dateCollection, IWorkload workload, int dayIndex, IList<DateOnly> filteredDates)
        {
            IList<IWorkloadDayBase> workloadDays = CollectStatistics(dateCollection, workload, new NoneWorkloadDayFilter<IWorkloadDayBase>());
            var filteredWorkloadDays = workloadDays.Where(d => !filteredDates.Contains(d.CurrentDate)).ToList();

            Statistic statistic = new Statistic(workload);
            statistic.CalculateCustomTemplateDay(filteredWorkloadDays, dayIndex);
        }

        public void LoadFilteredWorkloadDayTemplates(IList<DateOnlyPeriod> dateCollection, IWorkload workload, IList<DateOnly> filteredDates, int templateIndex)
        {
            var workloadDays = CollectStatistics(dateCollection, workload, new OutlierWorkloadDayFilter<IWorkloadDayBase>(workload, _outlierRepository));
            var filteredWorkloadDays = workloadDays.Where(d => !filteredDates.Contains(d.CurrentDate)).ToList();

            var statistic = new Statistic(workload);
            statistic.ReloadCustomTemplateDay(filteredWorkloadDays, templateIndex);
        }
    }
}
