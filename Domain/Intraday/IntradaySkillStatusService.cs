using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class IntradaySkillStatusService : IIntradaySkillStatusService
	{
		private readonly ISkillForecastedTasksProvider _skillForecastedTasksProvider;
		private readonly ISkillActualTasksProvider _skillActualTasksDetailProvider;
		private readonly ISkillRepository _skillRepository;

		public IntradaySkillStatusService(ISkillForecastedTasksProvider skillForecastedTasksProvider, ISkillActualTasksProvider skillActualTasksDetailProvider, ISkillRepository skillRepository)
		{
			_skillForecastedTasksProvider = skillForecastedTasksProvider;
			_skillActualTasksDetailProvider = skillActualTasksDetailProvider;
			_skillRepository = skillRepository;
		}

		public IEnumerable<SkillStatusModel> GetSkillStatusModels(DateTime now)
		{
			var skills = _skillRepository.FindSkillsWithAtLeastOneQueueSource();
			var skillsTimezone = skills.ToDictionary(skill => skill.Id.Value, skill => skill.TimeZone);
			var taskDetails = _skillForecastedTasksProvider.GetForecastedTasks(now);
			var actualTasks = _skillActualTasksDetailProvider.GetActualTasks(skillsTimezone);
			
			var ret = taskDetails.Select(task => new SkillStatusModel()
			{
				Measures = new List<SkillStatusMeasure>()
				{
					new SkillStatusMeasure()
					{
						Name = "Calls", StringValue = "No actual data found", Severity = 0, Value = 0, LatestDate = DateTime.MinValue
					}
				},
				Severity = 0, SkillName = task.SkillName
			}).ToList();

			foreach (var item in actualTasks)
			{
				var maxIntervalForSkill = item.IntervalTasks.Max(x => x.IntervalStart);
				var forecastedData = taskDetails.Where(s => s.SkillId == item.SkillId);
				var forecastedSum = 0.0;
				var actualSum = item.IntervalTasks.Sum(interval => interval.Task);
				if (forecastedData.Any())
				{
					forecastedSum =
						forecastedData.First()
							.IntervalTasks.Where(interval => interval.IntervalStart <= maxIntervalForSkill)
							.Sum(t => t.Task);
				}
				var relativeDifference = forecastedSum - actualSum;
				var message = "Below threshold";
				if (relativeDifference < -100)
				{
					message = "Exceeds threshold";
				}
				foreach (var skillStatus in ret)
				{
					if (skillStatus.SkillName == item.SkillName)
					{
						skillStatus.Measures = new List<SkillStatusMeasure>
						{
							new SkillStatusMeasure
							{
								Name = "Calls",
								Value = Math.Round(relativeDifference),
								Severity = 1,
								StringValue = message,
								LatestDate = TimeZoneHelper.ConvertFromUtc(maxIntervalForSkill, skillsTimezone[item.SkillId]),
								ActualCalls = actualSum,
								ForecastedCalls = Math.Round(forecastedSum)
							}
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
	}
}
				