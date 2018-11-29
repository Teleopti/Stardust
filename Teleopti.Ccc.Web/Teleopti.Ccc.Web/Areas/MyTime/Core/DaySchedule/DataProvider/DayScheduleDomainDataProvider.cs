using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.DaySchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.DaySchedule.DataProvider
{
	public class DayScheduleDomainDataProvider : IDayScheduleDomainDataProvider
	{
		private readonly IScheduleProvider _scheduleProvider;
		private readonly IProjectionProvider _projectionProvider;
		private readonly IPersonRequestProvider _personRequestProvider;
		private readonly ISeatOccupancyProvider _seatBookingProvider;
		private readonly IUserTimeZone _userTimeZone;
		private readonly INow _now;
		private readonly IPermissionProvider _permissionProvider;
		private readonly ILicenseAvailability _licenseAvailability;
		private readonly IAbsenceRequestProbabilityProvider _absenceRequestProbabilityProvider;

		public DayScheduleDomainDataProvider(IScheduleProvider scheduleProvider,
			IProjectionProvider projectionProvider,
			IPersonRequestProvider personRequestProvider,
			ISeatOccupancyProvider seatBookingProvider,
			IUserTimeZone userTimeZone,
			INow now,
			IPermissionProvider permissionProvider,
			IAbsenceRequestProbabilityProvider absenceRequestProbabilityProvider,
			ILicenseAvailability licenseAvailability)
		{
			_scheduleProvider = scheduleProvider;
			_projectionProvider = projectionProvider;
			_personRequestProvider = personRequestProvider;
			_seatBookingProvider = seatBookingProvider;
			_userTimeZone = userTimeZone;
			_now = now;
			_permissionProvider = permissionProvider;
			_absenceRequestProbabilityProvider = absenceRequestProbabilityProvider;
			_licenseAvailability = licenseAvailability;
		}

		public DayScheduleDomainData GetDaySchedule(DateOnly date)
		{
			var period = new DateOnlyPeriod(date, date);
			var scheduleDays = _scheduleProvider.GetScheduleForPeriod(new DateOnlyPeriod(period.StartDate.AddDays(-1), period.EndDate)).ToList();

			var personRequestPeriods = _personRequestProvider.RetrieveRequestPeriodsForLoggedOnUser(period);
			var requestProbability = _absenceRequestProbabilityProvider.GetAbsenceRequestProbabilityForPeriod(period);
			var seatBookings = _seatBookingProvider.GetSeatBookingsForScheduleDays(scheduleDays);

			var scheduleDaysAndProjections = scheduleDays.ToDictionary(day => day.DateOnlyAsPeriod.DateOnly,
				day => new ScheduleDaysAndProjection
				{
					ScheduleDay = day,
					Projection = _projectionProvider.Projection(day)
				});
			var timezone = _userTimeZone.TimeZone();
			var scheduleDayDomainData = new DayScheduleDomainData
			{
				Date = date,
				IsCurrentDay = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), timezone).Date == date.Date,
				MinMaxTime = getMinMaxTime(scheduleDaysAndProjections, date, timezone),
				ProbabilityClass = "",
				ProbabilityText = "",
			};

			if (scheduleDaysAndProjections.ContainsKey(date))
			{
				var theDay = scheduleDaysAndProjections[date];
				var scheduleDay = theDay.ScheduleDay;
				scheduleDayDomainData.ScheduleDay = scheduleDay;
				scheduleDayDomainData.Projection = theDay.Projection;
				scheduleDayDomainData.OvertimeAvailability = scheduleDay.OvertimeAvailablityCollection() == null
					? null
					: scheduleDay.OvertimeAvailablityCollection().FirstOrDefault();
			}

			var yesterdayOvertimeAvailability = scheduleDaysAndProjections[date.AddDays(-1)]?.ScheduleDay
				.OvertimeAvailablityCollection()?.FirstOrDefault();
			if (yesterdayOvertimeAvailability?.EndTime != null)
			{
				var endTimeSpan = yesterdayOvertimeAvailability.EndTime.Value;
				var yesterdayOvertimeAvailabilityEndTime = yesterdayOvertimeAvailability?.DateOfOvertime.Date.AddDays(endTimeSpan.Days).AddHours(endTimeSpan.Hours).AddMinutes(endTimeSpan.Minutes);

				var todayShiftStartTime = scheduleDaysAndProjections[date].Projection.Period()?.StartDateTime;

				if (yesterdayOvertimeAvailabilityEndTime > todayShiftStartTime)
				{
					scheduleDayDomainData.OvertimeAvailabilityYesterday = yesterdayOvertimeAvailability;
				}
			}

			if (personRequestPeriods != null)
			{
				scheduleDayDomainData.PersonRequestCount = personRequestPeriods
					.Where(r => TimeZoneInfo.ConvertTimeFromUtc(r.StartDateTime, timezone)
									.Date == date.Date)
					.ToArray().Length;
			}

			if (requestProbability != null)
			{
				var absenceRequestProbability = requestProbability.First(a => a.Date == date);
				scheduleDayDomainData.Availability = absenceRequestProbability.Availability;
				scheduleDayDomainData.ProbabilityClass = absenceRequestProbability.CssClass;
				scheduleDayDomainData.ProbabilityText = absenceRequestProbability.Text;
			}

			if (seatBookings != null)
			{
				scheduleDayDomainData.SeatBookingInformation = seatBookings
					.Where(seatBooking => seatBooking.BelongsToDate == date)
					.ToArray();
			}

			setPermissionsInfo(scheduleDayDomainData);

			return scheduleDayDomainData;
		}

		private void setPermissionsInfo(DayScheduleDomainData scheduleDayDomainData)
		{
			scheduleDayDomainData.AsmEnabled = _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger) && _licenseAvailability.IsLicenseEnabled(DefinedLicenseOptionPaths.TeleoptiCccAgentScheduleMessenger);
			scheduleDayDomainData.TextRequestPermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.TextRequests);
			scheduleDayDomainData.OvertimeAvailabilityPermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.OvertimeAvailabilityWeb);
			scheduleDayDomainData.AbsenceRequestPermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb);
			scheduleDayDomainData.OvertimeRequestPermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.OvertimeRequestWeb);
			scheduleDayDomainData.AbsenceReportPermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AbsenceReport);
			scheduleDayDomainData.ShiftTradeBulletinBoardPermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeBulletinBoard);
			scheduleDayDomainData.ShiftExchangePermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeBulletinBoard);
			scheduleDayDomainData.ShiftTradeRequestPermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb);
			scheduleDayDomainData.PersonAccountPermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewPersonalAccount);
			scheduleDayDomainData.ViewPossibilityPermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewStaffingInfo);
		}

		private TimePeriod getMinMaxTime(Dictionary<DateOnly, ScheduleDaysAndProjection> scheduleDaysAndProjections, DateOnly date, TimeZoneInfo timezone)
		{
			var start = new DateTime();
			var end = new DateTime();

			var hasLayerToday = scheduleDaysAndProjections[date].Projection != null &&
								scheduleDaysAndProjections[date].Projection.HasLayers;
			if (hasLayerToday)
			{
				var todayShiftStart = (DateTime)scheduleDaysAndProjections[date].Projection.Period()?.StartDateTime;
				var todayShiftEnd = (DateTime)scheduleDaysAndProjections[date].Projection.Period()?.EndDateTime;

				start = TimeZoneHelper.ConvertFromUtc(todayShiftStart, timezone);
				end = TimeZoneHelper.ConvertFromUtc(todayShiftEnd, timezone);
			}

			var hasOvertimeAvailabilityOnTodayOrYesterday =
				scheduleDaysAndProjections.Any(s => s.Value.ScheduleDay.OvertimeAvailablityCollection().Length > 0);
			if (hasOvertimeAvailabilityOnTodayOrYesterday)
			{
				var overtimeAvailabilityPeriod = getOvertimeAvailibilityPeriodInUtc(scheduleDaysAndProjections, date, timezone);
				if (overtimeAvailabilityPeriod != null)
				{
					var overtimeAvailabilityStart = TimeZoneHelper.ConvertFromUtc(overtimeAvailabilityPeriod.Value.StartDateTime, timezone);
					var overtimeAvailabilityEnd = TimeZoneHelper.ConvertFromUtc(overtimeAvailabilityPeriod.Value.EndDateTime, timezone);

					if (!hasLayerToday)
					{
						start = overtimeAvailabilityStart;
						end = overtimeAvailabilityEnd;
					}
					else
					{
						if (overtimeAvailabilityStart < start && overtimeAvailabilityStart.Date == start.Date)
							start = overtimeAvailabilityStart;

						if (overtimeAvailabilityEnd > end)
							end = overtimeAvailabilityEnd;
					}
				}
			}

			if (end > date.Date && end > start)
			{
				var adjustedStart = start > date.Date ? start : date.Date;

				var startTimeSpan = adjustTimeSpan(date, adjustedStart, start.TimeOfDay);
				var endTimeSpan = adjustTimeSpan(date, end, end.TimeOfDay);

				if (start.Date != end.Date && end.Date == date.Date)
				{
					startTimeSpan = date.Date.TimeOfDay;
				}

				var margin = TimeSpan.FromMinutes(ScheduleConsts.TimelineMarginInMinute);
				startTimeSpan = startTimeSpan == TimeSpan.Zero ? startTimeSpan : startTimeSpan.Subtract(margin);

				var endTimeSpanWithoutDays = endTimeSpan.Subtract(TimeSpan.FromDays(endTimeSpan.Days));
				endTimeSpan = endTimeSpanWithoutDays == TimeSpan.Zero ? endTimeSpan : endTimeSpan.Add(margin);

				return new TimePeriod(startTimeSpan, endTimeSpan);
			}

			return new TimePeriod(TimeSpan.Zero, TimeSpan.Zero);
		}

		private static TimeSpan adjustTimeSpan(DateOnly date, DateTime end, TimeSpan timeSpan)
		{
			var delta = end.Date - date.Date;

			if (delta.TotalMilliseconds > 0)
			{
				timeSpan = timeSpan.Add(delta);
			}

			return timeSpan;
		}

		private static DateTimePeriod? getOvertimeAvailibilityPeriodInUtc(Dictionary<DateOnly, ScheduleDaysAndProjection> scheduleDaysAndProjections, DateOnly date, TimeZoneInfo timezone)
		{
			var todayOvertimeAvailabilityPeriod = getOvertimeAvailabilityPeriodByDate(scheduleDaysAndProjections[date].ScheduleDay, timezone);

			var overnightOvertimeAvailabilityPeriod = getOvertimeAvailabilityPeriodByDate(scheduleDaysAndProjections[date.AddDays(-1)].ScheduleDay, timezone);

			if (todayOvertimeAvailabilityPeriod.HasValue)
			{
				var start = todayOvertimeAvailabilityPeriod.Value.StartDateTime;
				var end = todayOvertimeAvailabilityPeriod.Value.EndDateTime;

				if (overnightOvertimeAvailabilityPeriod.HasValue)
				{
					if (overnightOvertimeAvailabilityPeriod.Value.Intersect(todayOvertimeAvailabilityPeriod.Value) && overnightOvertimeAvailabilityPeriod.Value.EndDateTime > todayOvertimeAvailabilityPeriod.Value.EndDateTime)
					{
						end = overnightOvertimeAvailabilityPeriod.Value.EndDateTime;
					}
				}

				return new DateTimePeriod(start, end);
			}

			if (overnightOvertimeAvailabilityPeriod.HasValue)
			{
				var start = overnightOvertimeAvailabilityPeriod.Value.StartDateTime;
				var end = overnightOvertimeAvailabilityPeriod.Value.EndDateTime;

				return new DateTimePeriod(start, end);
			}

			return null;
		}

		private static DateTimePeriod? getOvertimeAvailabilityPeriodByDate(IScheduleDay scheduleDay, TimeZoneInfo timezone)
		{
			if (scheduleDay.OvertimeAvailablityCollection()?.Count() == 0) return null;

			var overtimeAvailabilityCollection = scheduleDay.OvertimeAvailablityCollection().OrderBy(o => o.StartTime);

			var date = scheduleDay.DateOnlyAsPeriod.DateOnly.Date;
			var overtimeAvailabilityPeriodStart = date.Add(overtimeAvailabilityCollection.FirstOrDefault().StartTime.Value);
			var overtimeAvailabilityPeriodEnd = date.Add(overtimeAvailabilityCollection.LastOrDefault().EndTime.Value);

			return new DateTimePeriod(TimeZoneHelper.ConvertToUtc(overtimeAvailabilityPeriodStart, timezone), TimeZoneHelper.ConvertToUtc(overtimeAvailabilityPeriodEnd, timezone));
		}

		internal class ScheduleDaysAndProjection
		{
			public IScheduleDay ScheduleDay { get; set; }
			public IVisualLayerCollection Projection { get; set; }
		}
	}
}