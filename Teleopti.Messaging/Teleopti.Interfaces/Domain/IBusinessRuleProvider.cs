using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IBusinessRuleProvider
	{
		INewBusinessRuleCollection GetAllBusinessRules(ISchedulingResultStateHolder schedulingResultStateHolder);

		INewBusinessRuleCollection GetBusinessRulesForShiftTradeRequest(
			ISchedulingResultStateHolder schedulingResultStateHolder, bool enableSiteOpenHoursRule);

		INewBusinessRuleCollection GetAllEnabledBusinessRulesForShiftTradeRequest(ISchedulingResultStateHolder schedulingResultStateHolder,
			bool enableSiteOpenHoursRule);

		IBusinessRuleResponse GetDeniableResponse(INewBusinessRuleCollection enabledRules, IList<IBusinessRuleResponse> ruleResponses);
	}
}