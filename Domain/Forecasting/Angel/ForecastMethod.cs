using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class ForecastMethod : IForecastMethod
	{
		private readonly IIndexVolumes _indexVolumes;

		public ForecastMethod(IIndexVolumes indexVolumes)
		{
			_indexVolumes = indexVolumes;
		}

		public IList<IForecastingTarget> Forecast(TaskOwnerPeriod historicalData, DateOnlyPeriod futurePeriod)
		{
			var indexes = _indexVolumes.Create(historicalData);
			return create(historicalData, indexes, futurePeriod);
		}

		private IList<IForecastingTarget> create(ITaskOwnerPeriod historicalDepth, IEnumerable<IVolumeYear> volumes, DateOnlyPeriod futurePeriod)
		{
			var averageStatistics = new AverageStatistics();
			if (historicalDepth.TaskOwnerDayCollection.Count > 0)
				averageStatistics.AverageTasks = historicalDepth.TotalStatisticCalculatedTasks / historicalDepth.TaskOwnerDayCollection.Count;
			else
				averageStatistics.AverageTasks = 0d;

			averageStatistics.TalkTime = historicalDepth.TotalStatisticAverageTaskTime;
			averageStatistics.AfterTalkTime = historicalDepth.TotalStatisticAverageAfterTaskTime;

			var targetForecastingList = new List<IForecastingTarget>();

			foreach (var day in futurePeriod.DayCollection())
			{
				var forecastingTarget = new ForecastingTarget(day, new OpenForWork(true, true));

				var totalDayItem = new TotalDayItem();

				SetComparison(forecastingTarget, totalDayItem, volumes, averageStatistics);
				targetForecastingList.Add(forecastingTarget);
			}

			return targetForecastingList;
		}

		private void SetComparison(IForecastingTarget day, TotalDayItem totalDayItem, IEnumerable<IVolumeYear> volumes, AverageStatistics averageStatistics)
		{
			double totalTaskIndex = 1;
			double talkTimeIndex = 1;
			double afterTalkTimeIndex = 1;

			//Accumulate the indexes
			foreach (var year in volumes)
			{
				totalTaskIndex *= year.TaskIndex(day.CurrentDate);
				talkTimeIndex *= year.TalkTimeIndex(day.CurrentDate);
				afterTalkTimeIndex *= year.AfterTalkTimeIndex(day.CurrentDate);
			}

			double currentTotalTaskIndex = totalDayItem.TaskIndex == 0 ? totalTaskIndex : totalDayItem.TaskIndex;
			double currentTalkTimeIndex = totalDayItem.TalkTimeIndex == 0 ? talkTimeIndex : totalDayItem.TalkTimeIndex;
			double currentAfterTalkTimeIndex = totalDayItem.AfterTalkTimeIndex == 0
				? afterTalkTimeIndex
				: totalDayItem.AfterTalkTimeIndex;
			if (day.OpenForWork.IsOpenForIncomingWork)
			{
				day.AverageTaskTime = new TimeSpan((long) (talkTimeIndex*averageStatistics.TalkTime.Ticks));
				day.AverageAfterTaskTime = new TimeSpan((long) (afterTalkTimeIndex*averageStatistics.AfterTalkTime.Ticks));
				day.Tasks = totalTaskIndex*averageStatistics.AverageTasks;
			}
			totalDayItem.SetComparisonValues(day, totalTaskIndex, talkTimeIndex, afterTalkTimeIndex, 1d);
			totalDayItem.TaskIndex = currentTotalTaskIndex;
			totalDayItem.TalkTimeIndex = currentTalkTimeIndex;
			totalDayItem.AfterTalkTimeIndex = currentAfterTalkTimeIndex;
		}
	}

	public interface IIndexVolumes
	{
		IEnumerable<IVolumeYear> Create(TaskOwnerPeriod historicalData);
	}

	public class IndexVolumes : IIndexVolumes
	{
		public IEnumerable<IVolumeYear> Create(TaskOwnerPeriod historicalData)
		{
			return new IVolumeYear[]
			{
				new MonthOfYear(historicalData, new MonthOfYearCreator()),
				new WeekOfMonth(historicalData, new WeekOfMonthCreator()),
				new DayOfWeeks(historicalData, new DaysOfWeekCreator())
			};
		}
	}
}