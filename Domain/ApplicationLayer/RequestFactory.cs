using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IRequestFactory
	{
		IRequestApprovalService GetRequestApprovalService(INewBusinessRuleCollection allNewRules, IScenario scenario);
		IShiftTradeRequestStatusChecker GetShiftTradeRequestStatusChecker();
	}

	public class RequestFactory : IRequestFactory
	{
		private readonly ISwapAndModifyService _swapAndModifyService;
		private readonly IPersonRequestCheckAuthorization _personRequestCheckAuthorization;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;

		public RequestFactory(ISwapAndModifyService swapAndModifyService, IPersonRequestCheckAuthorization personRequestCheckAuthorization, ISchedulingResultStateHolder schedulingResultStateHolder, IGlobalSettingDataRepository globalSettingDataRepository)
		{
			_swapAndModifyService = swapAndModifyService;
			_personRequestCheckAuthorization = personRequestCheckAuthorization;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_globalSettingDataRepository = globalSettingDataRepository;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public IRequestApprovalService GetRequestApprovalService(INewBusinessRuleCollection allNewRules, IScenario scenario)
		{
			return new RequestApprovalServiceScheduler(_schedulingResultStateHolder.Schedules,
																		 scenario, _swapAndModifyService, allNewRules, new ResourceCalculationOnlyScheduleDayChangeCallback(), _globalSettingDataRepository);
		}

		public IShiftTradeRequestStatusChecker GetShiftTradeRequestStatusChecker()
		{
			return new ShiftTradeRequestStatusCheckerWithSchedule(_schedulingResultStateHolder.Schedules, _personRequestCheckAuthorization);
		}
	}
}