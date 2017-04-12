﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
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
			if (ruleSet.TemplateGenerator.BaseActivity.ActivityCollection.Where(x => x.RequiresSkill)
				.Any(baseActivity => !skillList.Any(skill => skill.Activity.Equals(baseActivity))))
			{
				return false;
			}

			return ruleSet.ExtenderCollection
				.SelectMany(workShiftExtender => workShiftExtender.ExtendWithActivity.ActivityCollection)
				.Where(x => x.RequiresSkill)
				.All(baseExtendedActivity => skillList.Any(skill => skill.Activity.Equals(baseExtendedActivity)));
		}
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_MasterActivity_42795)]
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