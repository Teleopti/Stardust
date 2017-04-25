using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public class RuleSetBagExtractorProvider
	{
		public IRuleSetBagExtractor Fetch(SchedulingOptions schedulingOptions)
		{
			var fixedShiftBag = schedulingOptions.FixedShiftBag;
			return fixedShiftBag == null
				? (IRuleSetBagExtractor) new RuleSetBagExtractor()
				: new FixedRuleSetBagExtractor(fixedShiftBag);
		}
	}
}