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

		public IEnumerable<SkillStatusModel> GetSkillStatusModels()
		{
			var ret = new List<SkillStatusModel>();
			var taskDetails = _skillForecastedTasksProvider.GetForecastedTasks();
			var actualTasks = _skillActualTasksDetailProvider.GetActualTasks();
		
			foreach (var item in taskDetails)
			{
				var filteredActualTasks = actualTasks.Where(x => x.SkillId == item.SkillId).Select(y => y.IntervalTasks);
				var actualDetails = new List<IntervalTasks>();
				if (filteredActualTasks.Any())
					actualDetails = filteredActualTasks.First();
				var absDifference = getAbsDifference(
					taskDetails.Where(x => x.SkillId == item.SkillId).Select(y => y.IntervalTasks).First(),
					actualDetails);

				ret.Add(new SkillStatusModel
				{
					SkillName = item.SkillName,
					Measures = new List<SkillStatusMeasure> { new SkillStatusMeasure { Name = "Calls", Value = absDifference, Severity = 1 } }
				});
			}
			int i = 1;
			return  ret.OrderBy(x => x.Measures[0].Severity).Select(x => new SkillStatusModel()
			{
				SkillName = x.SkillName,
				Measures = x.Measures,
				Severity = i++
			}).OrderByDescending(y=>y.Severity);
		}

		private double getAbsDifference(IEnumerable<IntervalTasks> forecastedTasks, List<IntervalTasks> actualDetails)
		{
			var result = 0.0;
			foreach (var forecastedItem in forecastedTasks)
			{
				var actualTask = 0.0;
				var actualTaskOnInterval =
					actualDetails.FirstOrDefault(x => x.Interval.StartDateTime.Equals(forecastedItem.Interval.StartDateTime));

				if (actualTaskOnInterval != null)
					actualTask = actualTaskOnInterval.Task;

				result += Math.Abs(forecastedItem.Task - actualTask);
			}
			return result;
		}
	}
}