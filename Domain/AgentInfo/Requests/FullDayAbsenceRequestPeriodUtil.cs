using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public static class FullDayAbsenceRequestPeriodUtil
	{
		public static DateTimePeriod AdjustFullDayAbsencePeriodIfRequired(DateTimePeriod period, IPerson person,
		   IScheduleDay dayScheduleForAbsenceReqStart, IScheduleDay dayScheduleForAbsenceReqEnd, IGlobalSettingDataRepository globalSettingsDataRepository)
		{
			var fullDayTimeSpanStart = new TimeSpan(0, 0, 0);
			var fullDayTimeSpanEnd = new TimeSpan(23, 59, 0);
			var personTimeZone = person.PermissionInformation.DefaultTimeZone();
			var absencePeriodUserTime =
				new DateTimePeriod(
					DateTime.SpecifyKind(
						TimeZoneHelper.ConvertFromUtc(period.StartDateTime, personTimeZone),
						DateTimeKind.Utc),
					DateTime.SpecifyKind(
						TimeZoneHelper.ConvertFromUtc(period.EndDateTime, personTimeZone),
						DateTimeKind.Utc));

			bool isFullDayAbsenceRequest = (absencePeriodUserTime.StartDateTime.TimeOfDay == fullDayTimeSpanStart &&
											absencePeriodUserTime.EndDateTime.TimeOfDay == fullDayTimeSpanEnd);
			if (isFullDayAbsenceRequest)
			{
				var fullDayAbsenceRequestStartTimeSetting = globalSettingsDataRepository.FindValueByKey("FullDayAbsenceRequestStartTime",
						new TimeSpanSetting(new TimeSpan(0, 0, 0)));
				var fullDayAbsenceRequestEndTimeSetting = globalSettingsDataRepository.FindValueByKey("FullDayAbsenceRequestEndTime",
						new TimeSpanSetting(new TimeSpan(23, 59, 0)));

				var settingStartTime = fullDayAbsenceRequestStartTimeSetting.TimeSpanValue;
				var settingEndTime = fullDayAbsenceRequestEndTimeSetting.TimeSpanValue;

				var startDate = absencePeriodUserTime.StartDateTime.Date.Add(settingStartTime);
				var endDate = absencePeriodUserTime.EndDateTime.Date.Add(settingEndTime);

				var personAssignment = dayScheduleForAbsenceReqStart.PersonAssignment();
				if (dayScheduleForAbsenceReqStart.IsScheduled() && personAssignment != null && !dayScheduleForAbsenceReqStart.HasDayOff())
				{
					var dayScheduleStartTimeForAbsenceReqStart =
						personAssignment
							.Period.StartDateTimeLocal(personTimeZone);
                    startDate = dayScheduleStartTimeForAbsenceReqStart;
				}

				var assignment = dayScheduleForAbsenceReqEnd.PersonAssignment();
				if (dayScheduleForAbsenceReqEnd.IsScheduled() && assignment != null && !dayScheduleForAbsenceReqEnd.HasDayOff())
				{
					var dayScheduleEndTimeForAbsenceReqEnd = assignment
						.Period.EndDateTimeLocal(personTimeZone);
                    endDate = dayScheduleEndTimeForAbsenceReqEnd;
				}

				period =
					new DateTimePeriod(TimeZoneHelper.ConvertToUtc(startDate, personTimeZone),
						TimeZoneHelper.ConvertToUtc(endDate, personTimeZone));
			}
			return period;
		}

	}
}
