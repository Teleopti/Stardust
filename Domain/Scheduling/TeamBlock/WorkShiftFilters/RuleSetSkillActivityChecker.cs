

using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IRuleSetSkillActivityChecker
	{
		bool CheckSkillActivities(IWorkShiftRuleSet ruleSet, IEnumerable<ISkill> skillList);
	}

	public class RuleSetSkillActivityChecker : IRuleSetSkillActivityChecker
	{
		public bool CheckSkillActivities(IWorkShiftRuleSet ruleSet, IEnumerable<ISkill> skillList)
		{
			var baseActivity = ruleSet.TemplateGenerator.BaseActivity;
			var skills = skillList as IList<ISkill> ?? skillList.ToList();
			if (baseActivity.RequiresSkill)
			{
				bool found = false;
				foreach (var skill in skills)
				{
					if (skill.Activity == baseActivity)
						found = true;
				}

				if (!found)
					return false;
			}

			foreach (var workShiftExtender in ruleSet.ExtenderCollection)
			{
				if (workShiftExtender.ExtendWithActivity.RequiresSkill)
				{
					bool found = false;
					foreach (var skill in skills)
					{
						if (skill.Activity == workShiftExtender.ExtendWithActivity)
							found = true;
					}

					if (!found)
						return false;
				}
			}

			return true;
		}

	}
}