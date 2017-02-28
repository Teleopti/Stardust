using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IShiftTradeApproveService
	{
		IList<IBusinessRuleResponse> AutoApprove(IPersonRequest personRequest, IRequestApprovalService approvalService
			, ISchedulingResultStateHolder schedulingResultStateHolder);

		IList<IBusinessRuleResponse> SimulateApprove(IShiftTradeRequest shiftTradeRequest,
			INewBusinessRuleCollection allNewRules,
			ISchedulingResultStateHolder schedulingResultStateHolder);
	}
}
