using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public abstract class TeleoptiClassicBase : IForecastMethod
	{
		private readonly IIndexVolumes _indexVolumes;

		protected TeleoptiClassicBase(IIndexVolumes indexVolumes)
		{
			_indexVolumes = indexVolumes;
		}

		public IDictionary<DateOnly, double> ForecastTasks(ITaskOwnerPeriod historicalData, DateOnlyPeriod futurePeriod)
		{
			var dateAndTaskList = ForecastNumberOfTasks(historicalData, futurePeriod);

			return dateAndTaskList.ToDictionary(dateAndTask => dateAndTask.Date, dateAndTask => dateAndTask.Tasks);
		}

		public IDictionary<DateOnly, TimeSpan> ForecastTaskTime(ITaskOwnerPeriod historicalData, DateOnlyPeriod futurePeriod)
		{
			var taskTime = historicalData.TotalStatisticAverageTaskTime;
			var volumes = _indexVolumes.Create(historicalData).ToList();
			var result = new Dictionary<DateOnly, TimeSpan>();
			foreach (var day in futurePeriod.DayCollection())
			{
				double taskTimeIndex = 1;
				foreach (var volume in volumes)
				{
					taskTimeIndex *= volume.TaskTimeIndex(day);
				}
				
				result.Add(day, new TimeSpan((long)(taskTimeIndex * taskTime.Ticks)));
			}

			return result;
		}

		public IDictionary<DateOnly, TimeSpan> ForecastAfterTaskTime(ITaskOwnerPeriod historicalData, DateOnlyPeriod futurePeriod)
		{
			var afterTaskTime = historicalData.TotalStatisticAverageAfterTaskTime;
			var volumes = _indexVolumes.Create(historicalData).ToList();
			var result = new Dictionary<DateOnly, TimeSpan>();
			foreach (var day in futurePeriod.DayCollection())
			{
				double afterTaskTimeIndex = 1;
				foreach (var volume in volumes)
				{
					afterTaskTimeIndex *= volume.AfterTaskTimeIndex(day);
				}

				result.Add(day, new TimeSpan((long)(afterTaskTimeIndex * afterTaskTime.Ticks)));
			}

			return result;
		}

		public abstract ForecastMethodType Id { get; }

		public Dictionary<DateOnly, double> SeasonalVariationTasks(ITaskOwnerPeriod historicalData)
		{
			var futurePeriod = new DateOnlyPeriod(historicalData.StartDate, historicalData.EndDate);
			var taskNumbers = ForecastNumberOfTasks(historicalData, futurePeriod);
			return taskNumbers.ToDictionary(task => task.Date, task => task.Tasks);
		}

		public Dictionary<DateOnly, double> SeasonalVariationTaskTime(ITaskOwnerPeriod historicalData)
		{
			var futurePeriod = new DateOnlyPeriod(historicalData.StartDate, historicalData.EndDate);
			var taskTimeNumbers = ForecastTaskTime(historicalData, futurePeriod);
			return taskTimeNumbers.ToDictionary(task => task.Key, task => taskTimeNumbers[task.Key].TotalSeconds);
		}

		public Dictionary<DateOnly, double> SeasonalVariationAfterTaskTime(ITaskOwnerPeriod historicalData)
		{
			var futurePeriod = new DateOnlyPeriod(historicalData.StartDate, historicalData.EndDate);
			var afterTaskTimeNumbers = ForecastAfterTaskTime(historicalData, futurePeriod);
			return afterTaskTimeNumbers.ToDictionary(task => task.Key, task => afterTaskTimeNumbers[task.Key].TotalSeconds);
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