using System;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class ExpiredRequestValidator : IExpiredRequestValidator
	{
		private readonly IGlobalSettingDataRepository _globalSettingsDataRepository;
		private readonly INow _now;

		public ExpiredRequestValidator(IGlobalSettingDataRepository globalSettingsDataRepository, INow now)
		{
			_globalSettingsDataRepository = globalSettingsDataRepository;
			_now = now;
		}

		public IValidatedRequest ValidateExpiredRequest(IAbsenceRequest absenceRequest, IScheduleRange scheduleRange)
		{
			var person = absenceRequest.Person;

			var absenceRequestExpiredThreshold = person.WorkflowControlSet?.AbsenceRequestExpiredThreshold;
			if (absenceRequestExpiredThreshold == null)
				return ValidatedRequest.Valid;

			var period = absenceRequest.Period;
			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var localPeriod = period.ToDateOnlyPeriod(timeZone);
			var dayScheduleForAbsenceReqStart = scheduleRange.ScheduledDayCollection(localPeriod).ToDictionary(s => s.DateOnlyAsPeriod.DateOnly);
			
			period = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodIfRequired(period, person,
				dayScheduleForAbsenceReqStart[localPeriod.StartDate],
				dayScheduleForAbsenceReqStart[localPeriod.EndDate], _globalSettingsDataRepository);

			var isValid = validateRequestStartTime(period.StartDateTime, timeZone, absenceRequestExpiredThreshold.Value);

			if (!isValid)
			{
				var language = person.PermissionInformation.UICulture();
				var validationError =
					string.Format(UserTexts.Resources.ResourceManager.GetString("RequestDenyReasonRequestExpired", language),
						period.StartDateTimeLocal(timeZone),
						absenceRequestExpiredThreshold.GetValueOrDefault());
				return new ValidatedRequest { IsValid = false, ValidationErrors = validationError, DenyOption = PersonRequestDenyOption.RequestExpired };
			}

			return ValidatedRequest.Valid;
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