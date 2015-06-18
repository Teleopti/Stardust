using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public abstract class TeleoptiClassicBase : IForecastMethod
	{
		private readonly IIndexVolumes _indexVolumes;

		protected TeleoptiClassicBase(IIndexVolumes indexVolumes)
		{
			_indexVolumes = indexVolumes;
		}

		public virtual ForecastResult Forecast(ITaskOwnerPeriod historicalData, DateOnlyPeriod futurePeriod)
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

				double totalTaskIndex = 1;
				double talkTimeIndex = 1;
				double afterTalkTimeIndex = 1;

				foreach (var year in volumes)
				{
					totalTaskIndex *= year.TaskIndex(forecastingTarget.CurrentDate);
					talkTimeIndex *= year.TalkTimeIndex(forecastingTarget.CurrentDate);
					afterTalkTimeIndex *= year.AfterTalkTimeIndex(forecastingTarget.CurrentDate);
				}

				if (forecastingTarget.OpenForWork.IsOpenForIncomingWork)
				{
					forecastingTarget.Tasks = totalTaskIndex * averageStatistics.AverageTasks;
					forecastingTarget.AverageTaskTime = new TimeSpan((long)(talkTimeIndex * averageStatistics.TalkTime.Ticks));
					forecastingTarget.AverageAfterTaskTime = new TimeSpan((long)(afterTalkTimeIndex * averageStatistics.AfterTalkTime.Ticks));
				}

				targetForecastingList.Add(forecastingTarget);
			}

			return new ForecastResult
			{
				ForecastingTargets = targetForecastingList
			};
		}

		public abstract ForecastMethodType Id { get; }

		public ForecastResult SeasonalVariation(ITaskOwnerPeriod historicalData)
		{
			return Forecast(historicalData, new DateOnlyPeriod(historicalData.StartDate, historicalData.EndDate));
		}
	}
}