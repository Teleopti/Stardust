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

			var skillForBaseActivity = ruleSet.TemplateGenerator.BaseActivity.ActivityCollection.Any(
				activity => !activity.RequiresSkill || skillList.Any(skill => skill.Activity.Equals(activity)));

			if (!skillForBaseActivity)
				return false;

			return ruleSet.ExtenderCollection.SelectMany(workShiftExtender => workShiftExtender.ExtendWithActivity.ActivityCollection)
				.All(activity => !activity.RequiresSkill || skillList.Any(skill => skill.Activity.Equals(activity)));
		}
	}
}