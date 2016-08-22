using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
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
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;

		private IPersonAbsence _approvedPersonAbsence;

		public RequestApprovalServiceScheduler(IScheduleDictionary scheduleDictionary,
			IScenario scenario,
			ISwapAndModifyService swapAndModifyService,
			INewBusinessRuleCollection newBusinessRules,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			IGlobalSettingDataRepository globalSettingsDataRepository,
			IPersonAbsenceAccountRepository personAbsenceAccountRepository)
		{
			_scenario = scenario;
			_swapAndModifyService = swapAndModifyService;
			_scheduleDictionary = scheduleDictionary;
			_newBusinessRules = newBusinessRules;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_globalSettingsDataRepository = globalSettingsDataRepository;
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
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

			var scheduleDaysForCheckingAccount = getScheduleDaysForCheckingAccount(absence, dayScheduleForAbsenceReqStart, totalScheduleRange, person, period).ToList();

			IList<IBusinessRuleResponse> ret = new List<IBusinessRuleResponse>();

			//adjust the full day absence period start/end if there are shifts that already exist within the day schedule or if
			//there is a global setting that specifies the length of a full day absence
			period = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodIfRequired(period, person, dayScheduleForAbsenceReqStart, dayScheduleForAbsenceReqEnd, _globalSettingsDataRepository);

			if (dayScheduleForAbsenceReqStart.FullAccess)
			{
				var layer = new AbsenceLayer(absence, period);
				var personAbsence = new PersonAbsence(person, _scenario, layer, personRequest);

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
		public IEnumerable<IBusinessRuleResponse> ApproveShiftTrade(IShiftTradeRequest shiftTradeRequest)
		{
			return _swapAndModifyService.SwapShiftTradeSwapDetails(shiftTradeRequest.ShiftTradeSwapDetails,
																  _scheduleDictionary,
																   _newBusinessRules, new ScheduleTagSetter(NullScheduleTag.Instance));
		}

		public IPersonAbsence GetApprovedPersonAbsence()
		{
			return _approvedPersonAbsence;
		}

		/// <summary>
		/// Get scheduleDays for checking account
		/// </summary>
		/// <remarks>To be more efficient, we only return the first schedule day for each personal account for <see cref="Teleopti.Ccc.Domain.Scheduling.Rules.NewPersonAccountRule"/></remarks>
		/// <returns></returns>
		private IEnumerable<IScheduleDay> getScheduleDaysForCheckingAccount(IAbsence absence,
			IScheduleDay dayScheduleForAbsenceReqStart,
			IScheduleRange totalScheduleRange, IPerson person, DateTimePeriod period)
		{
			if (_newBusinessRules.Item(typeof(NewPersonAccountRule)) == null)
			{
				return new [] {dayScheduleForAbsenceReqStart};
			}

			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var startDate = new DateOnly(period.StartDateTimeLocal(timeZone));
			var endDate = new DateOnly(period.EndDateTimeLocal(timeZone));

			if (startDate == endDate)
			{
				return new[] { dayScheduleForAbsenceReqStart };
			}

			var scheduleDays = new List<IScheduleDay>();
			var personAccounts = _personAbsenceAccountRepository.Find(person);
			var days = period.ToDateOnlyPeriod(timeZone).DayCollection();
			var checkedAccounts = new HashSet<IAccount>();
			var checkedScheduleDays = new HashSet<DateOnly>();

			foreach (var day in days)
			{
				var account = personAccounts.Find(absence, day);

				if (account == null)
					continue;

				if (checkedAccounts.Add(account) && checkedScheduleDays.Add(day))
				{
					scheduleDays.Add(totalScheduleRange.ScheduledDay(day));
				}
			}

			return scheduleDays;
		}
	}
}
