using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class IntradaySkillStatusService : IIntradaySkillStatusService
	{
		private readonly ISkillForecastedTasksProvider _skillForecastedTasksProvider;
		private readonly ISkillActualTasksProvider _skillActualTasksDetailProvider;

		public IntradaySkillStatusService(ISkillForecastedTasksProvider skillForecastedTasksProvider, ISkillActualTasksProvider skillActualTasksDetailProvider)
		{
			_skillForecastedTasksProvider = skillForecastedTasksProvider;
			_skillActualTasksDetailProvider = skillActualTasksDetailProvider;
		}

		public IEnumerable<SkillStatusModel> GetSkillStatusModels(DateTime now)
		{
			var taskDetails = _skillForecastedTasksProvider.GetForecastedTasks(now);
			var actualTasks = _skillActualTasksDetailProvider.GetActualTasks();
			var ret = taskDetails.Select(task => new SkillStatusModel()
			{
				Measures = new List<SkillStatusMeasure>()
				{
					new SkillStatusMeasure()
					{
						Name = "Calls", StringValue = "No actual data found.", Severity = 0, Value = 0, LatestDate = DateTime.MinValue
					}
				},
				Severity = 0, SkillName = task.SkillName
			}).ToList();

			foreach (var item in actualTasks)
			{
				var filteredTaskDetails = taskDetails.Where(x => x.SkillId == item.SkillId).Select(y => y.IntervalTasks);
				var taskDetail = new List<IntervalTasks>();
				if (filteredTaskDetails.Any())
					taskDetail = filteredTaskDetails.First();
				var absDifference = getAbsDifference(
					taskDetail.Where(x=>x.IntervalStart <= item.IntervalTasks.Max(u=>u.IntervalStart) ), actualTasks.Where(x => x.SkillId == item.SkillId).Select(y => y.IntervalTasks).First());
				//this is just for testing
				var message = "Below threshold";
				if (absDifference > 100)
				{
					message = "Exceeds threshold";
				}
				foreach (var skillStatus in ret)
				{
					if (skillStatus.SkillName == item.SkillName)
					{
						var lastestDataDate = actualTasks.Where(x => x.SkillId == item.SkillId).Select(y => y.IntervalTasks.Max(z => z.IntervalStart)).First();
						skillStatus.Measures = new List<SkillStatusMeasure>
						{
							new SkillStatusMeasure {Name = "Calls", Value = Math.Round(absDifference), Severity = 1, StringValue = message, LatestDate = lastestDataDate }
						};
					}
				}
			}
			int i = 1;
			return ret.OrderBy(x => x.Measures[0].Severity).Select(x => new SkillStatusModel()
			{
				SkillName = x.SkillName,
				Measures = x.Measures,
				Severity = i++
			}).OrderByDescending(y => y.Severity);
		}

		private double getAbsDifference(IEnumerable<IntervalTasks> forecastedTasks, List<IntervalTasks> actualDetails)
		{
			var result = 0.0;
			foreach (var forecastedItem in forecastedTasks)
			{
				var actualTask = 0.0;
				var actualTaskOnInterval =
					actualDetails.FirstOrDefault(x => x.IntervalStart.Equals(forecastedItem.IntervalStart));

				if (actualTaskOnInterval != null)
					actualTask = actualTaskOnInterval.Task;

				result += Math.Abs(forecastedItem.Task - actualTask);
			}
			return result;
		}

		private double getAbsDifference2(IEnumerable<IntervalTasks> forecastedTasks, List<IntervalTasks> actualDetails)
		{
			var result = 0.0;
			foreach (var actualDetail in actualDetails)
			{
				var forecastedTask = 0.0;
				var forecastedTaskOnInterval =
					forecastedTasks.FirstOrDefault(x => x.IntervalStart.Equals(actualDetail.IntervalStart));

				if (forecastedTaskOnInterval != null)
					forecastedTask = forecastedTaskOnInterval.Task;

				result += Math.Abs(actualDetail.Task - forecastedTask);
			}
			return result;
		}
	}
}
				