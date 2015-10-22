using System.Collections.Generic;
using System.Linq;
using Rhino.ServiceBus.DataStructures;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Intraday
{
	public class SkillForecastedTasksProvider : ISkillTasksDetailProvider
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

		public IDictionary<ISkill, IList<SkillTaskDetails>> GetForecastedTasks()
		{
			var allSkills = _skillRepository.FindSkillsWithAtLeastOneQueueSource();
			var skillToTaskData =  _skillDayRepository.GetSkillsTasksDetails(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), allSkills.ToList(),
				_scenarioRepository.LoadDefaultScenario());
			var result = new Dictionary<ISkill, IList<SkillTaskDetails>>();
			foreach (var item in skillToTaskData)
			{
				var skill = allSkills.First(x => x.Id == item.SkillId);
				if (!result.ContainsKey(skill))
				{
					result.Add(skill, new List<SkillTaskDetails>()); 
				}
				result[skill].Add(new SkillTaskDetails(){Interval = new DateTimePeriod(item.Minimum,item.Maximum),Task = item.TotalTasks});
			}
			return result;
		}
	}

	public interface ISkillTasksDetailProvider
	{
		IDictionary<ISkill, IList<SkillTaskDetails>> GetForecastedTasks();
	}

	public class SkillTaskDetails
	{
		public DateTimePeriod Interval { get; set; }
		public double Task { get; set; }
	}
}