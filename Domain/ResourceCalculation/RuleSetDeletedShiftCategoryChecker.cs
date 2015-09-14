using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IRuleSetDeletedShiftCategoryChecker
	{
		bool ContainsDeletedShiftCategory(IWorkShiftRuleSet ruleSet);
	}

	public class RuleSetDeletedShiftCategoryChecker : IRuleSetDeletedShiftCategoryChecker
	{
		public bool ContainsDeletedShiftCategory(IWorkShiftRuleSet ruleSet)
		{
			if (ruleSet == null)
				throw new ArgumentNullException("ruleSet");

			if (((IDeleteTag)ruleSet.TemplateGenerator.Category).IsDeleted)
				return true;

			return false;
		}
	}
}
