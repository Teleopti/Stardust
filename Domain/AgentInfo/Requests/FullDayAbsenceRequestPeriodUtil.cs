using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

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
			var localAbsencePeriodStart = period.StartDateTimeLocal(personTimeZone);
			var localAbsencePeriodEnd = period.EndDateTimeLocal(personTimeZone);

			bool isFullDayAbsenceRequest = (localAbsencePeriodStart.TimeOfDay == fullDayTimeSpanStart &&
											localAbsencePeriodEnd.TimeOfDay == fullDayTimeSpanEnd);
			if (isFullDayAbsenceRequest)
			{
				var fullDayAbsenceRequestStartTimeSetting = globalSettingsDataRepository.FindValueByKey("FullDayAbsenceRequestStartTime",
						new TimeSpanSetting(new TimeSpan(0, 0, 0)));
				var fullDayAbsenceRequestEndTimeSetting = globalSettingsDataRepository.FindValueByKey("FullDayAbsenceRequestEndTime",
						new TimeSpanSetting(new TimeSpan(23, 59, 0)));

				var settingStartTime = fullDayAbsenceRequestStartTimeSetting.TimeSpanValue;
				var settingEndTime = fullDayAbsenceRequestEndTimeSetting.TimeSpanValue;

				var startDate = localAbsencePeriodStart.Date.Add(settingStartTime);
				var endDate = localAbsencePeriodEnd.Date.Add(settingEndTime);

				var personAssignment = dayScheduleForAbsenceReqStart.PersonAssignment();
				if (dayScheduleForAbsenceReqStart.IsScheduled() && personAssignment != null && !dayScheduleForAbsenceReqStart.HasDayOff() && personAssignment.ShiftLayers.Any())
				{
					var dayScheduleStartTimeForAbsenceReqStart =
						personAssignment
							.Period.StartDateTimeLocal(personTimeZone);
                    startDate = dayScheduleStartTimeForAbsenceReqStart;
				}

				var assignment = dayScheduleForAbsenceReqEnd.PersonAssignment();
				if (dayScheduleForAbsenceReqEnd.IsScheduled() && assignment != null && !dayScheduleForAbsenceReqEnd.HasDayOff() && assignment.ShiftLayers.Any())
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
