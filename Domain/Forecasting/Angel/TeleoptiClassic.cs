using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class TeleoptiClassic : IForecastMethod
	{
		private readonly IIndexVolumes _indexVolumes;

		public TeleoptiClassic(IIndexVolumes indexVolumes)
		{
			_indexVolumes = indexVolumes;
		}

		public ForecastMethodType Id
		{
			get { return ForecastMethodType.TeleoptiClassic; }
		}

		public IList<IForecastingTarget> Forecast(TaskOwnerPeriod historicalData, DateOnlyPeriod futurePeriod)
		{
			var volumes = _indexVolumes.Create(historicalData);
			var averageStatistics = new AverageStatistics();
			if (historicalData.TaskOwnerDayCollection.Count > 0)
				averageStatistics.AverageTasks = historicalData.TotalStatisticCalculatedTasks / historicalData.TaskOwnerDayCollection.Count;
			else
				averageStatistics.AverageTasks = 0d;

			averageStatistics.TalkTime = historicalData.TotalStatisticAverageTaskTime;
			averageStatistics.AfterTalkTime = historicalData.TotalStatisticAverageAfterTaskTime;

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
				day.AverageTaskTime = new TimeSpan((long)(talkTimeIndex * averageStatistics.TalkTime.Ticks));
				day.AverageAfterTaskTime = new TimeSpan((long)(afterTalkTimeIndex * averageStatistics.AfterTalkTime.Ticks));
				day.Tasks = totalTaskIndex * averageStatistics.AverageTasks;
			}
			totalDayItem.SetComparisonValues(day, totalTaskIndex, talkTimeIndex, afterTalkTimeIndex, 1d);
			totalDayItem.TaskIndex = currentTotalTaskIndex;
			totalDayItem.TalkTimeIndex = currentTalkTimeIndex;
			totalDayItem.AfterTalkTimeIndex = currentAfterTalkTimeIndex;
		}
	}
}