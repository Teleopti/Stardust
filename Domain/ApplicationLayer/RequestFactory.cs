using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IRequestFactory
	{
		IRequestApprovalService GetRequestApprovalService(INewBusinessRuleCollection allNewRules, IScenario scenario, ISchedulingResultStateHolder schedulingResultStateHolder);
		IShiftTradeRequestStatusChecker GetShiftTradeRequestStatusChecker(ISchedulingResultStateHolder schedulingResultStateHolder);
	}

	public class RequestFactory : IRequestFactory
	{
		private readonly ISwapAndModifyService _swapAndModifyService;
		private readonly IPersonRequestCheckAuthorization _personRequestCheckAuthorization;
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;
		private readonly ICheckingPersonalAccountDaysProvider _checkingPersonalAccountDaysProvider;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;

		public RequestFactory(ISwapAndModifyService swapAndModifyService, 
							IPersonRequestCheckAuthorization personRequestCheckAuthorization,  
							IGlobalSettingDataRepository globalSettingDataRepository, 
							ICheckingPersonalAccountDaysProvider checkingPersonalAccountDaysProvider,
							IScheduleDayChangeCallback scheduleDayChangeCallback)
		{
			_swapAndModifyService = swapAndModifyService;
			_personRequestCheckAuthorization = personRequestCheckAuthorization;
			_globalSettingDataRepository = globalSettingDataRepository;
			_checkingPersonalAccountDaysProvider = checkingPersonalAccountDaysProvider;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods",
			MessageId = "1")]
		public IRequestApprovalService GetRequestApprovalService(INewBusinessRuleCollection allNewRules, IScenario scenario,
			ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			return new RequestApprovalServiceScheduler(schedulingResultStateHolder.Schedules,
				scenario, _swapAndModifyService, allNewRules, _scheduleDayChangeCallback,
				_globalSettingDataRepository, _checkingPersonalAccountDaysProvider);
		}

		public IShiftTradeRequestStatusChecker GetShiftTradeRequestStatusChecker(ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			return new ShiftTradeRequestStatusCheckerWithSchedule(schedulingResultStateHolder.Schedules, _personRequestCheckAuthorization);
		}
	}
}