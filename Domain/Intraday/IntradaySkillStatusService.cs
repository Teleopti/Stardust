using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class IntradaySkillStatusService : IIntradaySkillStatusService
	{
		private readonly ISkillForecastedTasksDetailProvider _skillForecastedTasksDetailProvider;
		private readonly ISkillActualTasksProvider _skillActualTasksDetailProvider;

		public IntradaySkillStatusService(ISkillForecastedTasksDetailProvider skillForecastedTasksDetailProvider, ISkillActualTasksProvider skillActualTasksDetailProvider)
		{
			_skillForecastedTasksDetailProvider = skillForecastedTasksDetailProvider;
			_skillActualTasksDetailProvider = skillActualTasksDetailProvider;
		}

		public IEnumerable<SkillStatusModel> GetSkillStatusModels()
		{
			var ret = new List<SkillStatusModel>();
			var taskDetails = _skillForecastedTasksDetailProvider.GetForecastedTasks();
			var actualTasks = _skillActualTasksDetailProvider.GetActualTasks();
		
			foreach (var item in taskDetails)
			{
				var absDifference = getAbsDifference(
					taskDetails.Where(x => x.SkillId == item.SkillId).Select(y => y.IntervalTasks).First(),
					actualTasks.Where(x => x.SkillId == item.SkillId).Select(y => y.IntervalTasks).First());

				ret.Add(new SkillStatusModel
				{
					SkillName = item.SkillName,
					Measures = new List<SkillStatusMeasure> { new SkillStatusMeasure { Name = "Calls", Value = absDifference, Severity = 1 } }
				});
			}

			return ret;
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