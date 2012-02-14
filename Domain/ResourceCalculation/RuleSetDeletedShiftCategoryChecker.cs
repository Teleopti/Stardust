using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IRuleSetDeletedShiftCategoryChecker
	{
		bool ContainsDeletedActivity(IWorkShiftRuleSet ruleSet);
	}

	public class RuleSetDeletedShiftCategoryChecker : IRuleSetDeletedShiftCategoryChecker
	{
		public bool ContainsDeletedActivity(IWorkShiftRuleSet ruleSet)
		{
			if (ruleSet == null)
				throw new ArgumentNullException("ruleSet");

			if (((IDeleteTag)ruleSet.TemplateGenerator.Category).IsDeleted)
				return true;

			return false;
		}
	}
}
