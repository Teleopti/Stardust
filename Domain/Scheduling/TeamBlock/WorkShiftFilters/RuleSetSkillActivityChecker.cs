using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
			var baseActivities = ruleSet.TemplateGenerator.BaseActivity.ActivityCollection;
			foreach (var baseActivity in baseActivities.Where(x => x.RequiresSkill))
			{
				if (!skillList.Any(skill => skill.Activity.Equals(baseActivity)))
					return false;
			}

			foreach (var workShiftExtender in ruleSet.ExtenderCollection)
			{
				var baseExtendedActivities = workShiftExtender.ExtendWithActivity.ActivityCollection;
				if (workShiftExtender.ExtendWithActivity.RequiresSkill)
				{
					foreach (var baseExtendedActivity in baseExtendedActivities)
					{
						if (!skillList.Any(skill => skill.Activity.Equals(baseExtendedActivity)))
							return false;
					}	
				}
			}

			return true;
		}
	}


	public class RuleSetSkillActivityCheckerOLD : IRuleSetSkillActivityChecker
	{
		public bool CheckSkillActivities(IWorkShiftRuleSet ruleSet, IEnumerable<ISkill> skillList)
		{
			var baseActivity = ruleSet.TemplateGenerator.BaseActivity;
			var skills = skillList as IList<ISkill> ?? skillList.ToList();
			if (baseActivity.RequiresSkill)
			{
				if (!skills.Any(skill => skill.Activity.Equals(baseActivity)))
					return false;
			}

			foreach (var workShiftExtender in ruleSet.ExtenderCollection)
			{
				if (workShiftExtender.ExtendWithActivity.RequiresSkill)
				{
					if (!skills.Any(skill => skill.Activity.Equals(workShiftExtender.ExtendWithActivity)))
						return false;
				}
			}

			return true;
		}
	}

}