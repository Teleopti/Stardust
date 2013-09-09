using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class RequestFactory : IRequestFactory
	{
	    private readonly ISwapAndModifyService _swapAndModifyService;
	    private readonly IPersonRequestCheckAuthorization _personRequestCheckAuthorization;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;

		public RequestFactory(ISwapAndModifyService swapAndModifyService, IPersonRequestCheckAuthorization personRequestCheckAuthorization, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
	        _swapAndModifyService = swapAndModifyService;
	        _personRequestCheckAuthorization = personRequestCheckAuthorization;
	    	_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public IRequestApprovalService GetRequestApprovalService(INewBusinessRuleCollection allNewRules, IScenario scenario)
		{
			return new RequestApprovalServiceScheduler(_schedulingResultStateHolder.Schedules, 
													   scenario, _swapAndModifyService,  allNewRules, new ResourceCalculationOnlyScheduleDayChangeCallback());
		}

		public IShiftTradeRequestStatusChecker GetShiftTradeRequestStatusChecker()
		{
			return new ShiftTradeRequestStatusCheckerWithSchedule(_schedulingResultStateHolder.Schedules, _personRequestCheckAuthorization);
		}
	}
}