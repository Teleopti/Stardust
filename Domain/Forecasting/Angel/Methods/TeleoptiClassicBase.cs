using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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

		public virtual IList<IForecastingTarget> Forecast(ITaskOwnerPeriod historicalData, DateOnlyPeriod futurePeriod)
		{
			var averageTasks = historicalData.TaskOwnerDayCollection.Count > 0
				? historicalData.TotalStatisticCalculatedTasks / historicalData.TaskOwnerDayCollection.Count
				: 0d;
			var talkTime = historicalData.TotalStatisticAverageTaskTime;
			var afterTalkTime = historicalData.TotalStatisticAverageAfterTaskTime;

			var volumes = _indexVolumes.Create(historicalData).ToList();
			var targetForecastingList = new List<IForecastingTarget>();
			foreach (var day in futurePeriod.DayCollection())
			{
				var forecastingTarget = new ForecastingTarget(day, new OpenForWork(true, true));
				if (!forecastingTarget.OpenForWork.IsOpenForIncomingWork)
				{
					targetForecastingList.Add(forecastingTarget);
					continue;
				}

				// Calculate pattern
				double totalTaskIndex = 1;
				double talkTimeIndex = 1;
				double afterTalkTimeIndex = 1;
				foreach (var year in volumes)
				{
					totalTaskIndex *= year.TaskIndex(day);
					talkTimeIndex *= year.TalkTimeIndex(day);
					afterTalkTimeIndex *= year.AfterTalkTimeIndex(day);
				}

				forecastingTarget.Tasks = totalTaskIndex * averageTasks;
				forecastingTarget.AverageTaskTime = new TimeSpan((long) (talkTimeIndex * talkTime.Ticks));
				forecastingTarget.AverageAfterTaskTime = new TimeSpan((long) (afterTalkTimeIndex * afterTalkTime.Ticks));

				targetForecastingList.Add(forecastingTarget);
			}

			return targetForecastingList;
		}

		public abstract ForecastMethodType Id { get; }

		public IEnumerable<DateAndTask> SeasonalVariation(ITaskOwnerPeriod historicalData)
		{
			var futurePeriod = new DateOnlyPeriod(historicalData.StartDate, historicalData.EndDate);
			return ForecastNumberOfTasks(historicalData, futurePeriod);
		}

		protected virtual IEnumerable<DateAndTask> ForecastNumberOfTasks(ITaskOwnerPeriod historicalData, DateOnlyPeriod futurePeriod)
		{
			var averageTasks = historicalData.TaskOwnerDayCollection.Count > 0
				? historicalData.TotalStatisticCalculatedTasks / historicalData.TaskOwnerDayCollection.Count
				: 0d;

			var volumes = _indexVolumes.Create(historicalData).ToList();
			var result = new List<DateAndTask>();
			foreach (var day in futurePeriod.DayCollection())
			{
				double totalTaskIndex = 1;
				foreach (var volume in volumes)
				{
					totalTaskIndex = totalTaskIndex * volume.TaskIndex(day);
				}

				var tasks = totalTaskIndex * averageTasks;

				result.Add(new DateAndTask
				{
					Date = day,
					Tasks = tasks
				});
			}

			return result;
		}
	}
}