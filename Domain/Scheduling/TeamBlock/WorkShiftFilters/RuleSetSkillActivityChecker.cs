

using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IRuleSetSkillActivityChecker
	{
		bool CheckSkillActivties(IWorkShiftRuleSet ruleSet, IEnumerable<IPersonSkill> personSkills);
	}

	public class RuleSetSkillActivityChecker : IRuleSetSkillActivityChecker
	{
		 public bool CheckSkillActivties(IWorkShiftRuleSet ruleSet, IEnumerable<IPersonSkill> personSkills)
		 {
			 var baseActivity = ruleSet.TemplateGenerator.BaseActivity;
			 if (baseActivity.RequiresSkill)
			 {
				 bool found = false;
				 foreach (var personSkill in personSkills)
				 {
					 if (personSkill.Skill.Activity == baseActivity)
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
					 foreach (var personSkill in personSkills)
					 {
						 if (personSkill.Skill.Activity == workShiftExtender.ExtendWithActivity)
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