using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public static class FullDayAbsenceRequestPeriodUtil
	{
		private static readonly TimeSpan fullDayTimeSpanStart = new TimeSpan(0, 0, 0);
		private static readonly TimeSpan fullDayTimeSpanEnd = new TimeSpan(23, 59, 0);

		public static DateTimePeriod AdjustFullDayAbsencePeriodIfRequired(DateTimePeriod period, IPerson person,
		   IScheduleDay dayScheduleForAbsenceReqStart, IScheduleDay dayScheduleForAbsenceReqEnd, IGlobalSettingDataRepository globalSettingsDataRepository)
		{
			var personTimeZone = person.PermissionInformation.DefaultTimeZone();
			var localAbsencePeriodStart = period.StartDateTimeLocal(personTimeZone);
			var localAbsencePeriodEnd = period.EndDateTimeLocal(personTimeZone);

			bool isFullDayAbsenceRequest = (localAbsencePeriodStart.TimeOfDay == fullDayTimeSpanStart &&
											localAbsencePeriodEnd.TimeOfDay == fullDayTimeSpanEnd);
			if (isFullDayAbsenceRequest)
			{
				var fullDayAbsenceRequestStartTimeSetting = globalSettingsDataRepository.FindValueByKey("FullDayAbsenceRequestStartTime",
						new TimeSpanSetting(fullDayTimeSpanStart));
				var fullDayAbsenceRequestEndTimeSetting = globalSettingsDataRepository.FindValueByKey("FullDayAbsenceRequestEndTime",
						new TimeSpanSetting(fullDayTimeSpanEnd));

				var settingStartTime = fullDayAbsenceRequestStartTimeSetting.TimeSpanValue;
				var settingEndTime = fullDayAbsenceRequestEndTimeSetting.TimeSpanValue;

				var startDate = localAbsencePeriodStart.Date.Add(settingStartTime);
				var endDate = localAbsencePeriodEnd.Date.Add(settingEndTime);

				var personAssignment = dayScheduleForAbsenceReqStart.PersonAssignment();
				if (dayScheduleForAbsenceReqStart.IsScheduled() && personAssignment != null && !dayScheduleForAbsenceReqStart.HasDayOff() && personAssignment.ShiftLayers.Any())
				{
					var dayScheduleStartTimeForAbsenceReqStart = personAssignment.Period.StartDateTimeLocal(personTimeZone);
                    startDate = dayScheduleStartTimeForAbsenceReqStart;
				}

				var assignment = dayScheduleForAbsenceReqEnd.PersonAssignment();
				if (dayScheduleForAbsenceReqEnd.IsScheduled() && assignment != null && !dayScheduleForAbsenceReqEnd.HasDayOff() && assignment.ShiftLayers.Any())
				{
					var dayScheduleEndTimeForAbsenceReqEnd = assignment.Period.EndDateTimeLocal(personTimeZone);
                    endDate = dayScheduleEndTimeForAbsenceReqEnd;
				}

				period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startDate, endDate, personTimeZone);
			}
			return period;
		}

		public static DateTimePeriod AdjustFullDayAbsencePeriodForOvernightShift(IAbsenceRequest request,
			List<IScheduleDay> scheduleDays, IGlobalSettingDataRepository globalSettingsDataRepository)
		{
			if (!request.FullDay || scheduleDays.Count<2)
				return request.Period;

			var adjustedPeriod =
				adjustRequestPeriodByGlobalFullDayAbseceSettings(request, globalSettingsDataRepository);

			var startDate = adjustRequestStartTime(scheduleDays.First(), adjustedPeriod.StartDateTime);
			var endDate = adjustRequestEndTime(scheduleDays.Last(), adjustedPeriod.EndDateTime);

			return new DateTimePeriod(startDate, endDate);
		}

		private static DateTimePeriod adjustRequestPeriodByGlobalFullDayAbseceSettings(IAbsenceRequest request, IGlobalSettingDataRepository globalSettingsDataRepository)
		{
			var personTimeZone = request.Person.PermissionInformation.DefaultTimeZone();
			var fullDayAbsenceRequestStartTimeSetting = globalSettingsDataRepository.FindValueByKey(
				"FullDayAbsenceRequestStartTime",
				new TimeSpanSetting(fullDayTimeSpanStart));
			var fullDayAbsenceRequestEndTimeSetting = globalSettingsDataRepository.FindValueByKey(
				"FullDayAbsenceRequestEndTime",
				new TimeSpanSetting(fullDayTimeSpanEnd));

			var settingStartTime = fullDayAbsenceRequestStartTimeSetting.TimeSpanValue;
			var settingEndTime = fullDayAbsenceRequestEndTimeSetting.TimeSpanValue;

			var startDate = request.Period.StartDateTime.ToDateOnly().Date.Add(settingStartTime);
			var endDate = request.Period.EndDateTime.ToDateOnly().Date.Add(settingEndTime);

			return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startDate, endDate, personTimeZone);
		}

		private static DateTime adjustRequestStartTime(IScheduleDay scheduleDay, DateTime originStartTime)
		{
			var personAssignment = scheduleDay.PersonAssignment();
			if (hasSchduleAndPersonAssignment(scheduleDay, personAssignment) && !personAssignment.BelongsToPeriod(originStartTime.ToDateOnly().ToDateOnlyPeriod()))
			{
				return  personAssignment.Period.EndDateTime;
			}

			return originStartTime;
		}

		private static DateTime adjustRequestEndTime(IScheduleDay scheduleDay, DateTime originEndTime)
		{
			var personAssignment = scheduleDay.PersonAssignment();
			if (hasSchduleAndPersonAssignment(scheduleDay, personAssignment))
			{
				return personAssignment.Period.EndDateTime;
			}

			return originEndTime;
		}

		private static bool hasSchduleAndPersonAssignment(IScheduleDay scheduleDay, IPersonAssignment personAssignment)
		{
			return scheduleDay.IsScheduled()  && !scheduleDay.HasDayOff() &&  personAssignment != null &&
				   personAssignment.ShiftLayers.Any();
		}
	}
}
