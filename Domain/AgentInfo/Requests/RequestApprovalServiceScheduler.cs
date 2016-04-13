using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class RequestApprovalServiceScheduler : IRequestApprovalService
	{
		private readonly IScenario _scenario;
		private readonly ISwapAndModifyService _swapAndModifyService;
		private readonly IScheduleDictionary _scheduleDictionary;
		private readonly INewBusinessRuleCollection _newBusinessRules;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly IGlobalSettingDataRepository _globalSettingsDataRepository;

		public RequestApprovalServiceScheduler(IScheduleDictionary scheduleDictionary,
													IScenario scenario,
													ISwapAndModifyService swapAndModifyService,
													INewBusinessRuleCollection newBusinessRules,
													IScheduleDayChangeCallback scheduleDayChangeCallback,
													IGlobalSettingDataRepository globalSettingsDataRepository
												)
		{
			_scenario = scenario;
			_swapAndModifyService = swapAndModifyService;
			_scheduleDictionary = scheduleDictionary;
			_newBusinessRules = newBusinessRules;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_globalSettingsDataRepository = globalSettingsDataRepository;

		}

		public IEnumerable<IBusinessRuleResponse> ApproveAbsence(IAbsence absence, DateTimePeriod period, IPerson person, IAbsenceRequest absenceRequest = null)
		{
			IScheduleRange totalScheduleRange = _scheduleDictionary[person];
			IScheduleDay dayScheduleForAbsenceReqStart =
				totalScheduleRange.ScheduledDay(
					new DateOnly(period.StartDateTimeLocal(person.PermissionInformation.DefaultTimeZone())));
			IScheduleDay dayScheduleForAbsenceReqEnd =
				totalScheduleRange.ScheduledDay(
					new DateOnly(period.EndDateTimeLocal(person.PermissionInformation.DefaultTimeZone())));

			IList<IBusinessRuleResponse> ret = new List<IBusinessRuleResponse>();

			//adjust the full day absence period start/end if there are shifts that already exist within the day schedule or if
			//there is a global setting that specifies the length of a full day absence
			period = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodIfRequired(period, person, dayScheduleForAbsenceReqStart, dayScheduleForAbsenceReqEnd, _globalSettingsDataRepository);

			if (dayScheduleForAbsenceReqStart.FullAccess)
			{
				var layer = new AbsenceLayer(absence, period);
				var personAbsence = new PersonAbsence(person, _scenario, layer, absenceRequest);

				dayScheduleForAbsenceReqStart.Add(personAbsence);

				var result = _scheduleDictionary.Modify(ScheduleModifier.Request, dayScheduleForAbsenceReqStart, _newBusinessRules, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));
				if (!result.IsEmpty())
				{
					// Why this call again? None is overridden before
					result = _scheduleDictionary.Modify(ScheduleModifier.Request, dayScheduleForAbsenceReqStart, _newBusinessRules, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));
				}

				foreach (var response in result)
				{
					if (!response.Overridden)
						ret.Add(response);
				}
				return ret;
			}
			// this can probably not happen
			// Anyway, not full access is not an error that can be overridden
			return new List<IBusinessRuleResponse>();
		}
		public IEnumerable<IBusinessRuleResponse> ApproveShiftTrade(IShiftTradeRequest shiftTradeRequest)
		{
			return _swapAndModifyService.SwapShiftTradeSwapDetails(shiftTradeRequest.ShiftTradeSwapDetails,
																  _scheduleDictionary,
																   _newBusinessRules, new ScheduleTagSetter(NullScheduleTag.Instance));
		}
	}
}
