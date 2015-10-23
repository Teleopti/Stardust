using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class IntradaySkillStatusService : IIntradaySkillStatusService
	{
		private readonly ISkillForecastedTasksDetailProvider _skillForecastedTasksDetailProvider;

		public IntradaySkillStatusService(ISkillForecastedTasksDetailProvider skillForecastedTasksDetailProvider)
		{
			_skillForecastedTasksDetailProvider = skillForecastedTasksDetailProvider;
		}

		public IEnumerable<SkillStatusModel> GetSkillStatusModels()
		{
			var ret = new List<SkillStatusModel>();
			var taskDetails = _skillForecastedTasksDetailProvider.GetForecastedTasks();

			foreach (KeyValuePair<ISkill, IList<SkillTaskDetails>> pair in taskDetails)
			{
				var skill = pair.Key;
				var values = pair.Value;
				var sumvalues = values.Sum(tasks => tasks.Task);
				ret.Add(new SkillStatusModel { SkillName = skill.Name, Measures = new List<SkillStatusMeasure> { new SkillStatusMeasure { Name = "Calls", Value = sumvalues, Severity = 1 } } });
			}

			return ret;
		}
	}
}