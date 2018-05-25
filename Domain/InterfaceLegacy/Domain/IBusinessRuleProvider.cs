using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IBusinessRuleProvider
	{
		INewBusinessRuleCollection GetBusinessRulesForShiftTradeRequest(
			ISchedulingResultStateHolder schedulingResultStateHolder, bool enableSiteOpenHoursRule);

		INewBusinessRuleCollection GetAllEnabledBusinessRulesForShiftTradeRequest(
			ISchedulingResultStateHolder schedulingResultStateHolder,
			bool enableSiteOpenHoursRule);

		IBusinessRuleResponse GetFirstDeniableResponse(INewBusinessRuleCollection enabledRules,
			IList<IBusinessRuleResponse> ruleResponses);
	}
}