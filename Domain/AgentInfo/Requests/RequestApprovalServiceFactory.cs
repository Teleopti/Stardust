using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class RequestApprovalServiceFactory : IRequestApprovalServiceFactory
	{
		private readonly ISwapAndModifyService _swapAndModifyService;
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;
		private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
		private readonly ICheckingPersonalAccountDaysProvider _checkingPersonalAccountDaysProvider;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly IPersonRequestCheckAuthorization _personRequestCheckAuthorization;

		public RequestApprovalServiceFactory(ISwapAndModifyService swapAndModifyService, IGlobalSettingDataRepository globalSettingDataRepository, IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate, ICheckingPersonalAccountDaysProvider checkingPersonalAccountDaysProvider, IScheduleDayChangeCallback scheduleDayChangeCallback, IPersonRequestCheckAuthorization personRequestCheckAuthorization)
		{
			_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
			_checkingPersonalAccountDaysProvider = checkingPersonalAccountDaysProvider;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_personRequestCheckAuthorization = personRequestCheckAuthorization;
			_swapAndModifyService = swapAndModifyService;
			_globalSettingDataRepository = globalSettingDataRepository;
		}

		public IRequestApprovalService MakeRequestApprovalService(IScheduleDictionary scheduleDictionary, IScenario scenario, IPersonRequest personRequest)
		{
			var requestType = personRequest.Request.RequestType;
			var scheduleRange = scheduleDictionary[personRequest.Person];
			var businessRules = _businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRange);

			switch (requestType)
			{
				case RequestType.AbsenceRequest:
					return new AbsenceRequestApprovalService(
						scenario,
						scheduleDictionary,
						businessRules,
						_scheduleDayChangeCallback,
						_globalSettingDataRepository,
						_checkingPersonalAccountDaysProvider);
				case RequestType.ShiftTradeRequest:
					return new ShiftTradeRequestApprovalService(
						scheduleDictionary,
						_swapAndModifyService,
						businessRules,
						_personRequestCheckAuthorization);
			}

			throw new NotSupportedException("RequestType is not supported :" + requestType);
		}
	}
}
