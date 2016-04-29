using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public class RuleSetBagExtractorProvider
	{
		public IRuleSetBagExtractor Fetch(ISchedulingOptions schedulingOptions)
		{
			var fixedShiftBag = schedulingOptions.FixedShiftBag;
			return fixedShiftBag == null
				? (IRuleSetBagExtractor) new RuleSetBagExtractor()
				: new FixedRuleSetBagExtractor(fixedShiftBag);
		}
	}
}