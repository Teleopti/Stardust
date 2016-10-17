using System;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class RequestExpirationValidator : IAbsenceRequestValidator
	{
		private readonly IGlobalSettingDataRepository _globalSettingsDataRepository;
		private readonly INow _now;

		public RequestExpirationValidator(INow now, IGlobalSettingDataRepository globalSettingsDataRepository)
		{
			_now = now;
			_globalSettingsDataRepository = globalSettingsDataRepository;
		}

		public string InvalidReason { get; }

		public string DisplayText { get; }

		public IValidatedRequest Validate(IAbsenceRequest absenceRequest,
			RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest)
		{
			var person = absenceRequest.Person;
			var absenceRequestExpiredThreshold = person.WorkflowControlSet.AbsenceRequestExpiredThreshold;
			if (!absenceRequestExpiredThreshold.HasValue)
				return new ValidatedRequest { IsValid = true, ValidationErrors = string.Empty };

			var period = absenceRequest.Period;
			var scheduleRange = requiredForHandlingAbsenceRequest.SchedulingResultStateHolder.Schedules[person];
			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var dayScheduleForAbsenceReqStart = scheduleRange.ScheduledDay(new DateOnly(period.StartDateTimeLocal(timeZone)));
			var dayScheduleForAbsenceReqEnd = scheduleRange.ScheduledDay(new DateOnly(period.EndDateTimeLocal(timeZone)));

			period = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodIfRequired(period, person,
				dayScheduleForAbsenceReqStart,
				dayScheduleForAbsenceReqEnd, _globalSettingsDataRepository);

			var isValid = validateRequestStartTime(period.StartDateTime, person.PermissionInformation.DefaultTimeZone(), absenceRequestExpiredThreshold.Value);

			if (!isValid)
			{
				var language = person.PermissionInformation.UICulture();
				var validationError =
					string.Format(UserTexts.Resources.ResourceManager.GetString("RequestDenyReasonRequestExpired", language),
						period.StartDateTimeLocal(timeZone),
						absenceRequestExpiredThreshold.GetValueOrDefault());
				return new ValidatedRequest {IsValid = false, ValidationErrors = validationError};
			}

			return new ValidatedRequest {IsValid = true, ValidationErrors = string.Empty};
		}

		public IAbsenceRequestValidator CreateInstance()
		{
			throw new NotImplementedException();
		}

		public override bool Equals(object obj)
		{
			var validator = obj as BudgetGroupHeadCountValidator;
			return validator != null;
		}

		public override int GetHashCode()
		{
			var result = GetType().GetHashCode();
			return result;
		}

		private bool validateRequestStartTime(DateTime requestStartTime, TimeZoneInfo timeZone, int expiredThreshold)
		{
			var now = _now.UtcDateTime();
			var localNowForTimeZone = now.Add(timeZone.GetUtcOffset(now));
			var localTimeForRequestStartTime = TimeZoneHelper.ConvertFromUtc(requestStartTime, timeZone);
			var minimumRequestStartTime = localNowForTimeZone.AddMinutes(expiredThreshold);
			return localTimeForRequestStartTime.CompareTo(minimumRequestStartTime) >= 0;
		}
	}
}
