using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
			var absencePeriodUserTime =
				new DateTimePeriod(
					DateTime.SpecifyKind(
						TimeZoneHelper.ConvertFromUtc(period.StartDateTime, person.PermissionInformation.DefaultTimeZone()),
						DateTimeKind.Utc),
					DateTime.SpecifyKind(
						TimeZoneHelper.ConvertFromUtc(period.EndDateTime, person.PermissionInformation.DefaultTimeZone()),
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

				var startDate = new DateTime(absencePeriodUserTime.StartDateTime.Year,
												absencePeriodUserTime.StartDateTime.Month,
												absencePeriodUserTime.StartDateTime.Day,
												settingStartTime.Hours, settingStartTime.Minutes, settingStartTime.Seconds);
				var endDate = new DateTime(absencePeriodUserTime.EndDateTime.Year,
											absencePeriodUserTime.EndDateTime.Month,
											absencePeriodUserTime.EndDateTime.Day,
											settingEndTime.Hours, settingEndTime.Minutes, settingEndTime.Seconds);

				if (dayScheduleForAbsenceReqStart.IsScheduled() && dayScheduleForAbsenceReqStart.PersonAssignment() != null)
				{
					var dayScheduleStartTimeForAbsenceReqStart =
						dayScheduleForAbsenceReqStart.PersonAssignment()
							.Period.StartDateTimeLocal(person.PermissionInformation.DefaultTimeZone());
					startDate = startDate < dayScheduleStartTimeForAbsenceReqStart ? startDate : dayScheduleStartTimeForAbsenceReqStart;
				}

				if (dayScheduleForAbsenceReqEnd.IsScheduled() && dayScheduleForAbsenceReqEnd.PersonAssignment() != null)
				{
					var dayScheduleEndTimeForAbsenceReqEnd = dayScheduleForAbsenceReqEnd.PersonAssignment()
						.Period.EndDateTimeLocal(person.PermissionInformation.DefaultTimeZone());
					endDate = endDate > dayScheduleEndTimeForAbsenceReqEnd ? endDate : dayScheduleEndTimeForAbsenceReqEnd;
				}

				period =
					new DateTimePeriod(
						DateTime.SpecifyKind(
							TimeZoneHelper.ConvertToUtc(startDate, person.PermissionInformation.DefaultTimeZone()),
							DateTimeKind.Utc),
						DateTime.SpecifyKind(
							TimeZoneHelper.ConvertToUtc(endDate, person.PermissionInformation.DefaultTimeZone()),
							DateTimeKind.Utc));
			}
			return period;
		}

	}
}
