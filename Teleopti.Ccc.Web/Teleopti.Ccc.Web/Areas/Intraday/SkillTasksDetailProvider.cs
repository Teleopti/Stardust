using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Intraday
{
	public class SkillTasksDetailProvider : ISkillTasksDetailProvider
	{
		private readonly ISkillRepository _skillRepository;
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IScenarioRepository _scenarioRepository;

		public SkillTasksDetailProvider(ISkillRepository skillRepository, ISkillDayRepository skillDayRepository, IScenarioRepository scenarioRepository)
		{
			_skillRepository = skillRepository;
			_skillDayRepository = skillDayRepository;
			_scenarioRepository = scenarioRepository;
			//can this be different?
		}

		public IDictionary<ISkill,IList<SkillTaskDetailsModel>> GetSkillTaskDetails()
		{
			var allSkills = _skillRepository.FindSkillsWithAtLeastOneQueueSource();
			var skillToTaskData =  _skillDayRepository.GetSkillsTasksDetails(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), allSkills.ToList(),
				_scenarioRepository.LoadDefaultScenario());
			var result = new Dictionary<ISkill, IList<SkillTaskDetailsModel>>();
			foreach (var item in skillToTaskData)
			{
				var skill = allSkills.First(x => x.Id == item.SkillId);
				if (!result.ContainsKey(skill))
				{
					result.Add(skill,new List<SkillTaskDetailsModel>()); 
				}
				result[skill].Add(item);
			}
			return result;
		}
	}

	public interface ISkillTasksDetailProvider
	{
		IDictionary<ISkill, IList<SkillTaskDetailsModel>> GetSkillTaskDetails();
	}
}