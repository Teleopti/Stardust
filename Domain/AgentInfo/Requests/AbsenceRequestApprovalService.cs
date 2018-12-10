using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.WorkflowControl;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public interface IAbsenceApprovalService
	{
		IPersonAbsence GetApprovedPersonAbsence();

		IEnumerable<IBusinessRuleResponse> Approve(IAbsence absence, DateTimePeriod period, IPerson person);
	}

	public class AbsenceRequestApprovalService : IRequestApprovalService, IAbsenceApprovalService
	{
		private readonly IScenario _scenario;
		private readonly IScheduleDictionary _scheduleDictionary;
		private readonly INewBusinessRuleCollection _newBusinessRules;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly IGlobalSettingDataRepository _globalSettingsDataRepository;
		private readonly ICheckingPersonalAccountDaysProvider _checkingPersonalAccountDaysProvider;

		private IPersonAbsence _approvedPersonAbsence;

		protected AbsenceRequestApprovalService() { }

		public AbsenceRequestApprovalService(IScenario scenario, IScheduleDictionary scheduleDictionary, 
			INewBusinessRuleCollection newBusinessRules, IScheduleDayChangeCallback scheduleDayChangeCallback, IGlobalSettingDataRepository globalSettingsDataRepository, 
			ICheckingPersonalAccountDaysProvider checkingPersonalAccountDaysProvider)
		{
			_scenario = scenario;
			_scheduleDictionary = scheduleDictionary;
			_newBusinessRules = newBusinessRules;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_globalSettingsDataRepository = globalSettingsDataRepository;
			_checkingPersonalAccountDaysProvider = checkingPersonalAccountDaysProvider;
		}

		public IEnumerable<IBusinessRuleResponse> Approve(IAbsence absence, DateTimePeriod period, IPerson person)
		{
			return approveAbsence(person, period, absence);
		}

		public IPersonAbsence GetApprovedPersonAbsence()
		{
			return _approvedPersonAbsence;
		}

		public IEnumerable<IBusinessRuleResponse> Approve(IRequest request)
		{
			if (!(request is IAbsenceRequest absenceRequest))
			{
				throw new InvalidCastException("Request type should be AbsenceRequest!");
			}

			var person = absenceRequest.Person;
			var period = absenceRequest.Period;
			var absence = absenceRequest.Absence;

			var useSystemAuthorization = absenceRequest.Person.WorkflowControlSet?.GetMergedAbsenceRequestOpenPeriod(absenceRequest).AbsenceRequestProcess.GetType() == typeof(GrantAbsenceRequest);

			return approveAbsence(person, period, absence, useSystemAuthorization);
		}

		private IEnumerable<IBusinessRuleResponse> approveAbsence(IPerson person, DateTimePeriod period, IAbsence absence, bool isSystemModifying = false)
		{
			var totalScheduleRange = _scheduleDictionary[person];
			var dateOnlyPeriod = period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone());
			var dayScheduleForAbsence =
				totalScheduleRange.ScheduledDayCollection(dateOnlyPeriod).ToDictionary(s => s.DateOnlyAsPeriod.DateOnly);

			// To be more efficient, we only return the first schedule day for each personal account for NewPersonAccountRule
			var firstDayOfAbsence = dayScheduleForAbsence[dateOnlyPeriod.StartDate];
			var scheduleDaysForCheckingAccount
				= getScheduleDaysForCheckingAccount(absence, firstDayOfAbsence, totalScheduleRange, person, period).ToList();

			var ret = new List<IBusinessRuleResponse>();

			//adjust the full day absence period start/end if there are shifts that already exist within the day schedule or if
			//there is a global setting that specifies the length of a full day absence
			period = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodIfRequired(period, person,
				firstDayOfAbsence, dayScheduleForAbsence[dateOnlyPeriod.EndDate],
				_globalSettingsDataRepository);

			if (firstDayOfAbsence.FullAccess)
			{
				var layer = new AbsenceLayer(absence, period);
				var personAbsence = new PersonAbsence(person, _scenario, layer);

				scheduleDaysForCheckingAccount.ForEach(s => s.Add(personAbsence));

				var result = _scheduleDictionary.Modify(ScheduleModifier.Request, scheduleDaysForCheckingAccount, _newBusinessRules,
					_scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance), false, isSystemModifying);

				if (result == null)
				{
					return ret;
				}

				if (!result.IsEmpty())
				{
					// Why this call again? None is overridden before
					result = _scheduleDictionary.Modify(ScheduleModifier.Request, scheduleDaysForCheckingAccount, _newBusinessRules,
						_scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));
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

		private IEnumerable<IScheduleDay> getScheduleDaysForCheckingAccount(IAbsence absence, IScheduleDay dayScheduleForAbsenceReqStart, IScheduleRange totalScheduleRange, IPerson person, DateTimePeriod period)
		{
			if (_newBusinessRules.Item(typeof(NewPersonAccountRule)) == null)
			{
				return new[] { dayScheduleForAbsenceReqStart };
			}

			var days = _checkingPersonalAccountDaysProvider.GetDays(absence, person, period);
			if (days.DayCount() == 1)
			{
				return new[] { dayScheduleForAbsenceReqStart };
			}

			var newPeriod = days.StartDate == dayScheduleForAbsenceReqStart.DateOnlyAsPeriod.DateOnly
				? new DateOnlyPeriod(days.StartDate.AddDays(1), days.EndDate)
				: days;
			var scheduleDays = new List<IScheduleDay> { dayScheduleForAbsenceReqStart };
			scheduleDays.AddRange(totalScheduleRange.ScheduledDayCollection(newPeriod));
			return scheduleDays;
		}
	}
}
