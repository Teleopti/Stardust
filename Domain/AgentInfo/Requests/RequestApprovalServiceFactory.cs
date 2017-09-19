﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
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
		private readonly IOvertimeRequestUnderStaffingSkillProvider _overtimeRequestUnderStaffingSkillProvider;
		private readonly IOvertimeRequestSkillProvider _overtimeRequestSkillProvider;
		private readonly ICommandDispatcher _commandDispatcher;

		public RequestApprovalServiceFactory(ISwapAndModifyService swapAndModifyService,
			IGlobalSettingDataRepository globalSettingDataRepository, 
			IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate,
			ICheckingPersonalAccountDaysProvider checkingPersonalAccountDaysProvider,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			IPersonRequestCheckAuthorization personRequestCheckAuthorization,
			IOvertimeRequestUnderStaffingSkillProvider overtimeRequestUnderStaffingSkillProvider,
			IOvertimeRequestSkillProvider overtimeRequestSkillProvider,
			ICommandDispatcher commandDispatcher)
		{
			_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
			_checkingPersonalAccountDaysProvider = checkingPersonalAccountDaysProvider;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_personRequestCheckAuthorization = personRequestCheckAuthorization;
			_overtimeRequestUnderStaffingSkillProvider = overtimeRequestUnderStaffingSkillProvider;
			_overtimeRequestSkillProvider = overtimeRequestSkillProvider;
			_commandDispatcher = commandDispatcher;
			_swapAndModifyService = swapAndModifyService;
			_globalSettingDataRepository = globalSettingDataRepository;
		}

		public IRequestApprovalService MakeAbsenceRequestApprovalService(IScheduleDictionary scheduleDictionary,
			IScenario scenario, IPerson person)
		{
			var scheduleRange = scheduleDictionary[person];
			var businessRules = _businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRange);

			return new AbsenceRequestApprovalService(
				scenario,
				scheduleDictionary,
				businessRules,
				_scheduleDayChangeCallback,
				_globalSettingDataRepository,
				_checkingPersonalAccountDaysProvider);
		}

		public IRequestApprovalService MakeShiftTradeRequestApprovalService(IScheduleDictionary scheduleDictionary, IPerson person)
		{
			var scheduleRange = scheduleDictionary[person];
			var businessRules = _businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRange);
			return new ShiftTradeRequestApprovalService(
				scheduleDictionary,
				_swapAndModifyService,
				businessRules,
				_personRequestCheckAuthorization);
		}


		public IRequestApprovalService MakeOvertimeRequestApprovalService(ISkill[] validatedSkills)
		{
			return new OvertimeRequestApprovalService(_overtimeRequestUnderStaffingSkillProvider, _overtimeRequestSkillProvider,
				_commandDispatcher, validatedSkills);
		}
	}
}
