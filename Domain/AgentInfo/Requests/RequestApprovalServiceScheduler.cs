using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.UserTexts;
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
		private readonly ICheckingPersonalAccountDaysProvider _checkingPersonalAccountDaysProvider;
		private readonly IPersonRequestCheckAuthorization _personRequestCheckAuthorization;

		private IPersonAbsence _approvedPersonAbsence;

		public RequestApprovalServiceScheduler(IScheduleDictionary scheduleDictionary,
			IScenario scenario,
			ISwapAndModifyService swapAndModifyService,
			INewBusinessRuleCollection newBusinessRules,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			IGlobalSettingDataRepository globalSettingsDataRepository,
			ICheckingPersonalAccountDaysProvider checkingPersonalAccountDaysProvider, IPersonRequestCheckAuthorization personRequestCheckAuthorization)
		{
			_scenario = scenario;
			_swapAndModifyService = swapAndModifyService;
			_scheduleDictionary = scheduleDictionary;
			_newBusinessRules = newBusinessRules;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_globalSettingsDataRepository = globalSettingsDataRepository;
			_checkingPersonalAccountDaysProvider = checkingPersonalAccountDaysProvider;
			_personRequestCheckAuthorization = personRequestCheckAuthorization;
		}

		public IEnumerable<IBusinessRuleResponse> ApproveAbsence(IAbsence absence, DateTimePeriod period, IPerson person, IPersonRequest personRequest =  null)
		{
			IScheduleRange totalScheduleRange = _scheduleDictionary[person];
			IScheduleDay dayScheduleForAbsenceReqStart =
				totalScheduleRange.ScheduledDay(
					new DateOnly(period.StartDateTimeLocal(person.PermissionInformation.DefaultTimeZone())));
			IScheduleDay dayScheduleForAbsenceReqEnd =
				totalScheduleRange.ScheduledDay(
					new DateOnly(period.EndDateTimeLocal(person.PermissionInformation.DefaultTimeZone())));

			// To be more efficient, we only return the first schedule day for each personal account for NewPersonAccountRule
			var scheduleDaysForCheckingAccount 
				= getScheduleDaysForCheckingAccount(absence, dayScheduleForAbsenceReqStart, totalScheduleRange, person, period).ToList();

			IList<IBusinessRuleResponse> ret = new List<IBusinessRuleResponse>();

			//adjust the full day absence period start/end if there are shifts that already exist within the day schedule or if
			//there is a global setting that specifies the length of a full day absence
			period = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodIfRequired(period, person, dayScheduleForAbsenceReqStart, dayScheduleForAbsenceReqEnd, _globalSettingsDataRepository);

			if (dayScheduleForAbsenceReqStart.FullAccess)
			{
				var layer = new AbsenceLayer(absence, period);
				var personAbsence = new PersonAbsence(person, _scenario, layer);

				scheduleDaysForCheckingAccount.ForEach(s => s.Add(personAbsence));

				var result = _scheduleDictionary.Modify(ScheduleModifier.Request, scheduleDaysForCheckingAccount, _newBusinessRules, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));
				if (!result.IsEmpty())
				{
					// Why this call again? None is overridden before
					result = _scheduleDictionary.Modify(ScheduleModifier.Request, scheduleDaysForCheckingAccount, _newBusinessRules, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));
				}
				
				foreach (var response in result)
				{
					if (!response.Overridden)
						ret.Add(response);
				}

				if (ret.Count == 0)
				{
					_approvedPersonAbsence = personAbsence;
				}

				return ret;
			}
			// this can probably not happen
			// Anyway, not full access is not an error that can be overridden
			return new List<IBusinessRuleResponse>();
		}

		public void ScheduleChangedCallback(IList<IScheduleDay> scheduleDaysBefore, IList<IScheduleDay> scheduleDaysAfter)
		{
			for (int i = 1; i < scheduleDaysBefore.Count(); i++)
			{
				var partBefore = scheduleDaysBefore[i];
				var partAfter = scheduleDaysAfter[i];
				_scheduleDayChangeCallback.ScheduleDayChanged(partBefore, partAfter);
			}
		}

		public IEnumerable<IBusinessRuleResponse> ApproveShiftTrade(IShiftTradeRequest shiftTradeRequest)
		{
			var shiftTradeRequestStatusChecker = new ShiftTradeRequestStatusCheckerWithSchedule(_scheduleDictionary, _personRequestCheckAuthorization);
			var shiftTradeStatus = shiftTradeRequest.GetShiftTradeStatus(shiftTradeRequestStatusChecker);
			if (shiftTradeStatus == ShiftTradeStatus.Referred)
			{
				var person = shiftTradeRequest.PersonFrom;
				return new []
				{
					new BusinessRuleResponse(null, Resources.TheScheduleHasChanged, true, true, shiftTradeRequest.Period
						, person, shiftTradeRequest.Period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone()), string.Empty)
				};
			}
			return _swapAndModifyService.SwapShiftTradeSwapDetails(shiftTradeRequest.ShiftTradeSwapDetails,
																  _scheduleDictionary,
																   _newBusinessRules, new ScheduleTagSetter(NullScheduleTag.Instance));
		}

		public IPersonAbsence GetApprovedPersonAbsence()
		{
			return _approvedPersonAbsence;
		}

		private IEnumerable<IScheduleDay> getScheduleDaysForCheckingAccount(IAbsence absence,
			IScheduleDay dayScheduleForAbsenceReqStart,
			IScheduleRange totalScheduleRange, IPerson person, DateTimePeriod period)
		{
			if (_newBusinessRules.Item(typeof(NewPersonAccountRule)) == null)
			{
				return new[] {dayScheduleForAbsenceReqStart};
			}

			var days = _checkingPersonalAccountDaysProvider.GetDays(absence, person, period).ToList();
			if (days.Count == 1)
			{
				return new[] {dayScheduleForAbsenceReqStart};
			}

			var startDate = new DateOnly(period.StartDateTimeLocal(person.PermissionInformation.DefaultTimeZone()));
			var scheduleDays = new List<IScheduleDay> {dayScheduleForAbsenceReqStart};
			foreach (var day in days)
			{
				if (day == startDate)
					continue;

				scheduleDays.Add(totalScheduleRange.ScheduledDay(day));
			}
			return scheduleDays;
		}
	}
}
