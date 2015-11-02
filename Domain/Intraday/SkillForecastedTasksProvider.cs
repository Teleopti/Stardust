using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class SkillForecastedTasksProvider : ISkillForecastedTasksProvider
	{
		private readonly ISkillRepository _skillRepository;
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IScenarioRepository _scenarioRepository;

		public SkillForecastedTasksProvider(ISkillRepository skillRepository, ISkillDayRepository skillDayRepository, IScenarioRepository scenarioRepository)
		{
			_skillRepository = skillRepository;
			_skillDayRepository = skillDayRepository;
			_scenarioRepository = scenarioRepository;
			//can this be different?
		}

		public IList<SkillTaskDetails> GetForecastedTasks(DateTime now)
		{
			var allSkills = _skillRepository.FindSkillsWithAtLeastOneQueueSource();
			var skillToTaskData = _skillDayRepository.GetSkillsTasksDetails(new DateTimePeriod(now.Date, now), allSkills.ToList(),
				_scenarioRepository.LoadDefaultScenario()).ToList();
			var result = new List<SkillTaskDetails>();
			foreach (var item in skillToTaskData)
			{
				if (!result.Contains(new SkillTaskDetails(){SkillId = item.SkillId}))
				{
					result.Add(new SkillTaskDetails()
					{
						SkillId = item.SkillId,
						SkillName = item.Name,
						IntervalTasks =
							skillToTaskData.Where(x => x.SkillId == item.SkillId)
								.Select(x => new IntervalTasks()
								{
									IntervalStart = x.Minimum,
									Task = x.TotalTasks
								}).ToList()
					});
				}
			}
			
			return result;
		}
	}
}