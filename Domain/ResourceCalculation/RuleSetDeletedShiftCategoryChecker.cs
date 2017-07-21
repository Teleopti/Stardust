using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

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
				throw new ArgumentNullException(nameof(ruleSet));

			if (((IDeleteTag)ruleSet.TemplateGenerator.Category).IsDeleted)
				return true;

			return false;
		}
	}
}
