using Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class BusinessRuleProvider : IBusinessRuleProvider
	{
		public INewBusinessRuleCollection GetAllBusinessRules(ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			return NewBusinessRuleCollection.All(schedulingResultStateHolder);
		}

		public INewBusinessRuleCollection GetBusinessRulesForShiftTradeRequest(ISchedulingResultStateHolder schedulingResultStateHolder,
			bool enableSiteOpenHoursRule)
		{
			var rules = NewBusinessRuleCollection.All(schedulingResultStateHolder);
			rules.Remove(typeof(NewPersonAccountRule));
			rules.Remove(typeof(OpenHoursRule));
			rules.Add(new NonMainShiftActivityRule());
			if (enableSiteOpenHoursRule)
				rules.Add(new SiteOpenHoursRule(new SiteOpenHoursSpecification()));

			return rules;
		}
	}
}