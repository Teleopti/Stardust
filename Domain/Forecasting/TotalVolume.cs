using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public interface ITotalVolume
	{
		/// <summary>
		/// Creates a TotalVolume object
		/// </summary>
		/// <param name="historicalDepth">The historical depth.</param>
		/// <param name="workloadDays">The workload days.</param>
		/// <param name="volumes">The volumes.</param>
		/// <param name="outliers">The outliers.</param>
		/// <param name="startDayTrendFactor">The start day trend factor.</param>
		/// <param name="dayTrendFactor">The day trend factor.</param>
		/// <param name="useTrend">if set to <c>true</c> [use trend].</param>
		/// <param name="workload">The Workload.</param>
		/// <remarks>
		/// Created by: peterwe
		/// Created date: 2008-04-01
		/// </remarks>
		void Create(ITaskOwnerPeriod historicalDepth, IEnumerable<ITaskOwner> workloadDays, 
		                            IList<IVolumeYear> volumes, IList<IOutlier> outliers, 
		                            double startDayTrendFactor, double dayTrendFactor, bool useTrend, IWorkload workload);
	}

	/// <summary>
    /// Holds the total volumes for the specified period of skillDays
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2008-04-01
    /// </remarks>
    public class TotalVolume : ITotalVolume
	{
        private IList<TotalDayItem> _totalDayItemCollection;
        private double _averageTasks;
        private TimeSpan _talkTime;
        private TimeSpan _afterTalkTime;
        private TaskOwnerHelper _workloadDayPeriod;
        private IDictionary<IOutlier, TaskOwnerPeriod> _outliersWithStatistics;
        private IDictionary<DateOnly, IOutlier> _outliersByDate;
        private IList<IVolumeYear> _volumes;
        private ITaskOwnerPeriod _historicalDepth;
        

        /// <summary>
        /// Creates a TotalVolume object
        /// </summary>
        /// <param name="historicalDepth">The historical depth.</param>
        /// <param name="workloadDays">The workload days.</param>
        /// <param name="volumes">The volumes.</param>
        /// <param name="outliers">The outliers.</param>
        /// <param name="startDayTrendFactor">The start day trend factor.</param>
        /// <param name="dayTrendFactor">The day trend factor.</param>
        /// <param name="useTrend">if set to <c>true</c> [use trend].</param>
        /// <param name="workload">The Workload.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-04-01
        /// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "7")]
		public void Create(ITaskOwnerPeriod historicalDepth, IEnumerable<ITaskOwner> workloadDays, 
            IList<IVolumeYear> volumes, IList<IOutlier> outliers, 
            double startDayTrendFactor, double dayTrendFactor, bool useTrend, IWorkload workload)
        {
            _workloadDayPeriod = new TaskOwnerHelper(new List<ITaskOwner>(workloadDays));
            _historicalDepth = historicalDepth;
            _volumes = volumes;
            InitializeOutlierData(outliers);

            var sortedWorkloadDays = _workloadDayPeriod.TaskOwnerDays.OrderBy(w => w.CurrentDate).ToList();
			if(!sortedWorkloadDays.Any()) return;

            //Create historical task owner period 
            historicalDepth = new TaskOwnerPeriod(historicalDepth.CurrentDate,
                                                  historicalDepth.TaskOwnerDayCollection.Where(
                                                      w =>
                                                      !_outliersByDate.ContainsKey(w.CurrentDate)).
                                                      ToList(), historicalDepth.TypeOfTaskOwnerPeriod);

            if (historicalDepth.TaskOwnerDayCollection.Count > 0)
                _averageTasks = historicalDepth.TotalStatisticCalculatedTasks / historicalDepth.TaskOwnerDayCollection.Count;
            else
                _averageTasks = 0d;

            _talkTime = historicalDepth.TotalStatisticAverageTaskTime;
            _afterTalkTime = historicalDepth.TotalStatisticAverageAfterTaskTime;
            _totalDayItemCollection = new List<TotalDayItem>();

            if (!useTrend)
            {
                startDayTrendFactor = 1d;
                dayTrendFactor = 1d;
            }

            _workloadDayPeriod.BeginUpdate();
            var firstDate = sortedWorkloadDays[0].CurrentDate;
            foreach (ITaskOwner day in sortedWorkloadDays)
            {
                int days = (int)day.CurrentDate.Subtract(firstDate).TotalDays;
                var trendFactor = startDayTrendFactor * Math.Pow(dayTrendFactor, days);

                var wlDay = day as WorkloadDay;

                var template = (IWorkloadDayTemplate) workload.GetTemplateAt(TemplateTarget.Workload, (int)day.CurrentDate.DayOfWeek);
                 
                if (wlDay.OpenForWork.IsOpen != template.OpenForWork.IsOpen)
                {
                    wlDay.ChangeOpenHours(template.OpenHourList);
                }
                
                var totalDayItem = new TotalDayItem();
                
                TaskOwnerPeriod statisticsForOutlier;
                IOutlier outlier;
                if (day.OpenForWork.IsOpen &&
                    _outliersByDate.TryGetValue(day.CurrentDate, out outlier) &&
                    _outliersWithStatistics.TryGetValue(outlier, out statisticsForOutlier) &&
                    statisticsForOutlier.TaskOwnerDayCollection.Count>0)
                {
                    SetComparisonFromOutlier(day, statisticsForOutlier, totalDayItem, trendFactor);
                }
                else
                {
                    SetComparison(day, totalDayItem, trendFactor);
                }

                _totalDayItemCollection.Add(totalDayItem);
            }
            _workloadDayPeriod.EndUpdate();
        }

		private void SetComparison(IForecastingTarget day, TotalDayItem totalDayItem, double trendFactor)
        {
            double totalTaskIndex = 1;
            double taskTimeIndex = 1;
            double afterTaskTimeIndex = 1;

            //Accumulate the indexes
            foreach (IVolumeYear year in _volumes)
            {
                totalTaskIndex *= year.TaskIndex(day.CurrentDate);
                taskTimeIndex *= year.TaskTimeIndex(day.CurrentDate);
                afterTaskTimeIndex *= year.AfterTaskTimeIndex(day.CurrentDate);
            }

            double currentTotalTaskIndex = totalDayItem.TaskIndex == 0 ? totalTaskIndex * trendFactor : totalDayItem.TaskIndex;
            double currentTalkTimeIndex = totalDayItem.TalkTimeIndex == 0 ? taskTimeIndex : totalDayItem.TalkTimeIndex;
            double currentAfterTalkTimeIndex = totalDayItem.AfterTalkTimeIndex == 0
                                                   ? afterTaskTimeIndex
                                                   : totalDayItem.AfterTalkTimeIndex;

            if (day.OpenForWork.IsOpenForIncomingWork)
            {
                    day.AverageTaskTime = new TimeSpan((long) (taskTimeIndex*_talkTime.Ticks));
                    day.AverageAfterTaskTime = new TimeSpan((long) (afterTaskTimeIndex*_afterTalkTime.Ticks));
                    day.Tasks = totalTaskIndex*_averageTasks;
            }
            
            totalDayItem.SetComparisonValues(day, totalTaskIndex, taskTimeIndex, afterTaskTimeIndex, trendFactor);
            totalDayItem.TaskIndex = currentTotalTaskIndex;
            totalDayItem.TalkTimeIndex = currentTalkTimeIndex;
            totalDayItem.AfterTalkTimeIndex = currentAfterTalkTimeIndex;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private void SetComparisonFromOutlier(IForecastingTarget day, TaskOwnerPeriod statisticsForOutlier, TotalDayItem totalDayItem, double trendFactor)
        {
            double totalTaskIndex = totalDayItem.TaskIndex == 0 ? 1 * trendFactor : totalDayItem.TaskIndex;
            double talkTimeIndex = totalDayItem.TalkTimeIndex == 0 ? 1 : totalDayItem.TalkTimeIndex;
            double afterTalkTimeIndex = totalDayItem.AfterTalkTimeIndex == 0 ? 1 : totalDayItem.AfterTalkTimeIndex;

            if (day.OpenForWork.IsOpenForIncomingWork)
            {
                day.AverageTaskTime = statisticsForOutlier.TotalStatisticAverageTaskTime;
                day.AverageAfterTaskTime = statisticsForOutlier.TotalStatisticAverageAfterTaskTime;
                day.Tasks = statisticsForOutlier.TotalStatisticCalculatedTasks/
                            statisticsForOutlier.TaskOwnerDayCollection.Count;
            }
            
            totalDayItem.SetComparisonValues(day, 1, 1, 1, trendFactor);
            totalDayItem.TaskIndex = totalTaskIndex;
            totalDayItem.TalkTimeIndex = talkTimeIndex;
            totalDayItem.AfterTalkTimeIndex = afterTalkTimeIndex;
        }

        /// <summary>
        /// Initializes the outlier data.
        /// </summary>
        /// <param name="outliers">The outliers.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-20
        /// </remarks>
        private void InitializeOutlierData(IList<IOutlier> outliers)
        {
            if (_historicalDepth.TaskOwnerDayCollection.Count == 0) return;
            var minHistoricDate = _historicalDepth.TaskOwnerDayCollection.Min(t => t.CurrentDate);
            var minWorkingDate = _workloadDayPeriod.TaskOwnerDays.Min(w => w.CurrentDate);
            if (minWorkingDate<minHistoricDate) minHistoricDate = minWorkingDate;

            var maxHistoricDate = _historicalDepth.TaskOwnerDayCollection.Max(t => t.CurrentDate);
            var maxWorkingDate = _workloadDayPeriod.TaskOwnerDays.Max(w => w.CurrentDate);
            if (maxWorkingDate<maxHistoricDate) maxWorkingDate = maxHistoricDate;

            DateOnlyPeriod wholePeriod = new DateOnlyPeriod(minHistoricDate,maxWorkingDate);

            _outliersWithStatistics = new Dictionary<IOutlier, TaskOwnerPeriod>();
            foreach (IOutlier outlier in outliers)
            {
                IList<DateOnly> dates = outlier.GetDatesByPeriod(wholePeriod);
                TaskOwnerPeriod taskOwnerPeriodHistory = new TaskOwnerPeriod(wholePeriod.StartDate,
                                                                             _historicalDepth.TaskOwnerDayCollection.
                                                                                 Where(
                                                                                 t =>
                                                                                 dates.Contains(t.CurrentDate)).
                                                                                 ToList(), TaskOwnerPeriodType.Other);
                if (taskOwnerPeriodHistory.TaskOwnerDayCollection.Count == 0) continue;
                _outliersWithStatistics.Add(outlier, taskOwnerPeriodHistory);
            }
            _outliersByDate = Outlier.GetOutliersByDates(
                wholePeriod,
                outliers);
        }

        /// <summary>
        /// Gets the total day item collection.
        /// </summary>
        /// <value>The total day item collection.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-04-01
        /// </remarks>
        public ReadOnlyCollection<TotalDayItem> TotalDayItemCollection
        {
            get { return new ReadOnlyCollection<TotalDayItem>(_totalDayItemCollection); }
        }

        /// <summary>
        /// Gets the skill days.
        /// </summary>
        /// <value>The skill days.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-04-01
        /// </remarks>
        public ReadOnlyCollection<ITaskOwner> WorkloadDayCollection
        {
            get { return new ReadOnlyCollection<ITaskOwner>(_workloadDayPeriod.TaskOwnerDays); }
        }

        /// <summary>
        /// Removes the outlier.
        /// </summary>
        /// <param name="outlier">The outlier.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-20
        /// </remarks>
        public void RemoveOutlier(IOutlier outlier)
        {
            IEnumerable<DateOnly> dates = (from od in _outliersByDate
                                          where od.Value.Equals(outlier)
                                          select od.Key).ToList();

            if (_outliersWithStatistics.ContainsKey(outlier))
            {
                IEnumerable<TotalDayItem> totalDayItems =
                    _totalDayItemCollection.Where(t => dates.Contains(t.CurrentDate));
                ResetTotalDayItemsIndex(totalDayItems);
                _outliersWithStatistics.Remove(outlier);
            }

            dates.ForEach(d => _outliersByDate.Remove(d));
        }

        /// <summary>
        /// Recalculates the outlier.
        /// </summary>
        /// <param name="outlier">The outlier.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-20
        /// </remarks>
        public void RecalculateOutlier(IOutlier outlier)
        {
            if (_historicalDepth.TaskOwnerDayCollection.Count == 0) return;
            if (_outliersWithStatistics.ContainsKey(outlier)) _outliersWithStatistics.Remove(outlier);

            DateOnlyPeriod wholePeriod = new DateOnlyPeriod(
                    _historicalDepth.TaskOwnerDayCollection.Min(t => t.CurrentDate),
                    _workloadDayPeriod.TaskOwnerDays.Max(w => w.CurrentDate));

            IList<DateOnly> dates = (from o in _outliersByDate
                                          where o.Value.Equals(outlier)
                                          select o.Key).ToList();

            IEnumerable<TotalDayItem> totalDayItems =
                _totalDayItemCollection.Where(t => dates.Contains(t.CurrentDate));
            ResetTotalDayItemsIndex(totalDayItems);
            ReplaceOutliersByDate(outlier, wholePeriod, dates);

            TaskOwnerPeriod taskOwnerPeriodHistory = new TaskOwnerPeriod(wholePeriod.StartDate,
                                                                         _historicalDepth.TaskOwnerDayCollection.Where(
                                                                             t =>
                                                                             dates.Contains(t.CurrentDate)).
                                                                             ToList(), TaskOwnerPeriodType.Other);
            _outliersWithStatistics.Add(outlier, taskOwnerPeriodHistory);
            dates = outlier.GetDatesByPeriod(wholePeriod);
            
            foreach (DateOnly date in dates)
            {
                //Find the TotalDayItem
                TotalDayItem totalDayItem = _totalDayItemCollection.FirstOrDefault(t => t.CurrentDate == date);
                if (totalDayItem != null)
                {
                    if (taskOwnerPeriodHistory.TaskOwnerDayCollection.Count > 0)
                    {
                        totalDayItem.TaskIndex = 1;
                        totalDayItem.TalkTimeIndex = 1;
                        totalDayItem.AfterTalkTimeIndex = 1;
                        SetComparisonFromOutlier(totalDayItem.TaskOwner, taskOwnerPeriodHistory, totalDayItem,
                                                 totalDayItem.DayTrendFactor);
                    }
                    else
                    {
                        SetComparison(totalDayItem.TaskOwner,totalDayItem,totalDayItem.DayTrendFactor);
                    }
                }
            }
        }

        private void ReplaceOutliersByDate(IOutlier outlier, DateOnlyPeriod wholePeriod, IEnumerable<DateOnly> dates)
        {
            _outliersByDate = _outliersByDate.Except(
                _outliersByDate.Where(o => dates.Contains(o.Key))).ToDictionary(o => o.Key, v => v.Value);

            _outliersByDate = _outliersByDate.Concat(
                Outlier.GetOutliersByDates(wholePeriod, new List<IOutlier> { outlier })).ToDictionary(o => o.Key, v => v.Value);
        }

        private void ResetTotalDayItemsIndex(IEnumerable<TotalDayItem> totalDayItems)
        {
            foreach (TotalDayItem totalDayItem in totalDayItems)
            {
                SetComparison(totalDayItem.TaskOwner,totalDayItem,totalDayItem.DayTrendFactor);
            }
        }
    }
}
