﻿using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class RequestApprovalServiceFactory : IRequestApprovalServiceFactory
	{
		private readonly ISwapAndModifyService _swapAndModifyService;
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;
		private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;

		public RequestApprovalServiceFactory(ISwapAndModifyService swapAndModifyService, IGlobalSettingDataRepository globalSettingDataRepository, IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate, IPersonAbsenceAccountRepository personAbsenceAccountRepository)
		{
			_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
			_swapAndModifyService = swapAndModifyService;
			_globalSettingDataRepository = globalSettingDataRepository;
		}

		public IRequestApprovalService MakeRequestApprovalServiceScheduler(IScheduleDictionary scheduleDictionary, IScenario scenario, IPerson person)
		{
			var scheduleRange = scheduleDictionary[person];
			var businessRules = _businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRange);
			
			return new RequestApprovalServiceScheduler(
				scheduleDictionary,
				scenario,
				_swapAndModifyService,
				businessRules,
				new ResourceCalculationOnlyScheduleDayChangeCallback(),
				_globalSettingDataRepository,
				_personAbsenceAccountRepository);
		}
	}
}
