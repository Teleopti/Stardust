using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IShiftTradePendingReasonsService
	{

		void SimulateApproveAndSetBusinessRuleResponsesOnFail(IShiftTradeRequest shiftTradeRequest, INewBusinessRuleCollection allNewRules, ISchedulingResultStateHolder schedulingResultStateHolder);
		void SetBrokenBusinessRulesFieldOnPersonRequest(IEnumerable<IBusinessRuleResponse> ruleRepsonses, IPersonRequest personRequest);
	}
}