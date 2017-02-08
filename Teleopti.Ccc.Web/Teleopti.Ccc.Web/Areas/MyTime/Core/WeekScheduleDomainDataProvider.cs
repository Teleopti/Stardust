﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	public class WeekScheduleDomainDataProvider : IWeekScheduleDomainDataProvider
	{
		private readonly IScheduleProvider _scheduleProvider;
		private readonly IProjectionProvider _projectionProvider;
		private readonly IPersonRequestProvider _personRequestProvider;
		private readonly ISeatOccupancyProvider _seatBookingProvider;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IPermissionProvider _permissionProvider;
		private readonly INow _now;
		private readonly IAbsenceRequestProbabilityProvider _absenceRequestProbabilityProvider;
		private readonly IToggleManager _toggleManager;
		private readonly IUserCulture _culture;

		public WeekScheduleDomainDataProvider(IScheduleProvider scheduleProvider, IProjectionProvider projectionProvider,
			IPersonRequestProvider personRequestProvider,
			ISeatOccupancyProvider seatBookingProvider,
			IUserTimeZone userTimeZone, IPermissionProvider permissionProvider, INow now,
			IAbsenceRequestProbabilityProvider absenceRequestProbabilityProvider, IToggleManager toggleManager, IUserCulture culture)
		{
			_scheduleProvider = scheduleProvider;
			_projectionProvider = projectionProvider;
			_personRequestProvider = personRequestProvider;
			_seatBookingProvider = seatBookingProvider;
			_userTimeZone = userTimeZone;
			_permissionProvider = permissionProvider;
			_now = now;
			_absenceRequestProbabilityProvider = absenceRequestProbabilityProvider;
			_toggleManager = toggleManager;
			_culture = culture;
		}

		internal class ScheduleDaysAndProjection
		{
			public IScheduleDay ScheduleDay { get; set; }
			public IVisualLayerCollection Projection { get; set; }
		}

		public WeekScheduleDomainData Get(DateOnly date)
		{
			var firstDayOfWeek = DateHelper.GetFirstDateInWeek(date, _culture.GetCulture().DateTimeFormat.FirstDayOfWeek);
			var week = new DateOnlyPeriod(firstDayOfWeek, firstDayOfWeek.AddDays(6));
			var weekWithPreviousDay = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), firstDayOfWeek.AddDays(6));

			var scheduleDays = _scheduleProvider.GetScheduleForPeriod(weekWithPreviousDay).ToList();
			var personRequests = _personRequestProvider.RetrieveRequestsForLoggedOnUser(week);
			var requestProbability = _absenceRequestProbabilityProvider.GetAbsenceRequestProbabilityForPeriod(week);

			var seatBookings = _seatBookingProvider.GetSeatBookingsForScheduleDays(scheduleDays);

			var scheduleDaysAndProjections = scheduleDays.ToDictionary(day => day.DateOnlyAsPeriod.DateOnly, day => new ScheduleDaysAndProjection
			{
				ScheduleDay = day,
				Projection = _projectionProvider.Projection(day)
			});
			var minMaxTime = getMinMaxTime(scheduleDaysAndProjections, week, firstDayOfWeek);

			var list = new List<WeekScheduleDayDomainData>();
			foreach (var day in firstDayOfWeek.DateRange(7))
			{
				var weekScheduleDayDomainData = new WeekScheduleDayDomainData
				{
					Date = day,
					MinMaxTime = minMaxTime,
					ProbabilityClass = "",
					ProbabilityText = "",
				};

				if (scheduleDaysAndProjections.ContainsKey(day))
				{
					var theDay = scheduleDaysAndProjections[day];
					var scheduleDay = theDay.ScheduleDay;
					weekScheduleDayDomainData.ScheduleDay = scheduleDay;
					weekScheduleDayDomainData.Projection = theDay.Projection;
					weekScheduleDayDomainData.OvertimeAvailability = scheduleDay.OvertimeAvailablityCollection() == null ? null : scheduleDay.OvertimeAvailablityCollection().FirstOrDefault();
				}

				if (scheduleDaysAndProjections.ContainsKey(day.AddDays(-1)))
				{
					var yesterday = scheduleDaysAndProjections[day.AddDays(-1)];
					var scheduleYesterday = yesterday.ScheduleDay;
					weekScheduleDayDomainData.ProjectionYesterday = yesterday.Projection;
					weekScheduleDayDomainData.OvertimeAvailabilityYesterday = scheduleYesterday.OvertimeAvailablityCollection() == null ? null : scheduleYesterday.OvertimeAvailablityCollection().FirstOrDefault();
				}

				if (personRequests != null)
				{
					weekScheduleDayDomainData.PersonRequests = (from i in personRequests
						where TimeZoneInfo.ConvertTimeFromUtc(i.Request.Period.StartDateTime, _userTimeZone.TimeZone()).Date == day.Date
						select i).ToArray();
				}

				if (requestProbability != null)
				{
					var absenceRequestProbability = requestProbability.First(a => a.Date == day);
					weekScheduleDayDomainData.Availability = absenceRequestProbability.Availability;
					weekScheduleDayDomainData.ProbabilityClass = absenceRequestProbability.CssClass;
					weekScheduleDayDomainData.ProbabilityText = absenceRequestProbability.Text;
				}

				if (seatBookings != null)
				{
					weekScheduleDayDomainData.SeatBookingInformation = seatBookings.Where(seatBooking => seatBooking.BelongsToDate == day).ToArray();
				}
				
				list.Add(weekScheduleDayDomainData);
			}
			var days = list.ToArray();

			var colorSource = new ScheduleColorSource
			{
				ScheduleDays = scheduleDays,
				Projections = (from d in days where d.Projection != null select d.Projection).ToArray()
			};

			var asmPermission = _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger);
			var textRequestPermission = _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.TextRequests);
			var overtimeAvailabilityPermission = _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.OvertimeAvailabilityWeb);
			var absenceRequestPermission = _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb);
			var absenceReportPermission = _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AbsenceReport);
			var shiftTradeBulletinBoardPermission = _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeBulletinBoard);
			var shiftExchangePermission = _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeBulletinBoard);
			var personalAccountPermission = _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewPersonalAccount);
			var viewPossibilityPermission = _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewPossibility);

			var isCurrentWeek = week.Contains(_now.LocalDateOnly());

			return new WeekScheduleDomainData
			{
				Date = date,
				Days = days,
				ColorSource = colorSource,
				MinMaxTime = minMaxTime,
				AsmPermission = asmPermission,
				TextRequestPermission = textRequestPermission,
				OvertimeAvailabilityPermission = overtimeAvailabilityPermission,
				AbsenceRequestPermission = absenceRequestPermission,
				AbsenceReportPermission = absenceReportPermission,
				ShiftExchangePermission = shiftExchangePermission,
				ShiftTradeBulletinBoardPermission = shiftTradeBulletinBoardPermission,
				PersonAccountPermission = personalAccountPermission,
				ViewPossibilityPermission = viewPossibilityPermission,
				IsCurrentWeek = isCurrentWeek
			};
		}

		private TimeSpan minFunction(ScheduleDaysAndProjection scheduleDayData, DateOnlyPeriod week, DateOnly firstDayOfWeek)
		{
			var scheduleDay = scheduleDayData.ScheduleDay;
			var projection = scheduleDayData.Projection;
			var period = projection.Period();
			var earlyStart = new TimeSpan(23, 59, 59);
			if (period != null && projection.HasLayers)
			{
				var userTimeZone = _userTimeZone.TimeZone();
				var startTime = period.Value.TimePeriod(userTimeZone).StartTime;
				var endTime = period.Value.TimePeriod(userTimeZone).EndTime;
				var localEndDate = new DateOnly(period.Value.EndDateTimeLocal(userTimeZone).Date);
				if (endTime.Days > startTime.Days && week.Contains(localEndDate))
					earlyStart = TimeSpan.Zero;
				else if (scheduleDay.DateOnlyAsPeriod.DateOnly != firstDayOfWeek.AddDays(-1))
					earlyStart = startTime;
			}

			var overtimeAvailabilityCollection = scheduleDay.OvertimeAvailablityCollection();
			if (overtimeAvailabilityCollection == null)
				return earlyStart;
			var overtimeAvailability = overtimeAvailabilityCollection.FirstOrDefault();
			if (overtimeAvailability == null)
				return earlyStart;
			var earlyStartOvertimeAvailability = new TimeSpan(23, 59, 59);
			var overtimeAvailabilityStart = overtimeAvailability.StartTime.Value;
			var overtimeAvailabilityEnd = overtimeAvailability.EndTime.Value;
			if (overtimeAvailabilityEnd.Days > overtimeAvailabilityStart.Days && week.Contains(scheduleDay.DateOnlyAsPeriod.DateOnly.AddDays(1)))
				earlyStartOvertimeAvailability = TimeSpan.Zero;
			else if (scheduleDay.DateOnlyAsPeriod.DateOnly != firstDayOfWeek.AddDays(-1))
				earlyStartOvertimeAvailability = overtimeAvailabilityStart;
			return earlyStart < earlyStartOvertimeAvailability ? earlyStart : earlyStartOvertimeAvailability;
		}

		private TimeSpan maxfunction(ScheduleDaysAndProjection scheduleDayData, DateOnly firstDayOfWeek)
		{
			var scheduleDay = scheduleDayData.ScheduleDay;
			var projection = scheduleDayData.Projection;
			var period = projection.Period();
			var lateEnd = TimeSpan.Zero;
			if (period != null && projection.HasLayers)
			{
				var userTimeZone = _userTimeZone.TimeZone();
				var startTime = period.Value.TimePeriod(userTimeZone).StartTime;
				var endTime = period.Value.TimePeriod(userTimeZone).EndTime;

				//for the day before current week, only if end time crosses midnihgt, 
				//then it is a valid end time to be carried over to first week day (endTime.Days == 1)
				if (scheduleDay.DateOnlyAsPeriod.DateOnly == firstDayOfWeek.AddDays(-1) && endTime.Days == 1)
					lateEnd = endTime - TimeSpan.FromDays(1);
				else if (scheduleDay.DateOnlyAsPeriod.DateOnly != firstDayOfWeek.AddDays(-1)) //for the days of current week
				{
					//if end time cross midnight, then max. time is of course used, otherwise use the end time as it is
					lateEnd = endTime.Days > startTime.Days ? new TimeSpan(23, 59, 59) : endTime;
				}
			}
			var overtimeAvailabilityCollection = scheduleDay.OvertimeAvailablityCollection();
			if (overtimeAvailabilityCollection == null)
				return lateEnd;
			var overtimeAvailability = overtimeAvailabilityCollection.FirstOrDefault();
			if (overtimeAvailability == null)
				return lateEnd;

			var lateEndOvertimeAvailability = TimeSpan.Zero;
			var overtimeAvailabilityStart = overtimeAvailability.StartTime.Value;
			var overtimeAvailabilityEnd = overtimeAvailability.EndTime.Value;

			//for the day before current week, only if end time of OT Availability crosses midnight (ie., overtimeAvailabilityEnd.Days == 1), 
			//then it is a valid end time to be carried over to the first week day 
			if (scheduleDay.DateOnlyAsPeriod.DateOnly == firstDayOfWeek.AddDays(-1) && overtimeAvailabilityEnd.Days == 1)
				lateEndOvertimeAvailability = overtimeAvailabilityEnd - TimeSpan.FromDays(1);
			else if (scheduleDay.DateOnlyAsPeriod.DateOnly != firstDayOfWeek.AddDays(-1))  //for the days of current week
			{
				//if end time of OT Availability crosses midnight, then max. time is of course used, otherwise use its end time as it is
				lateEndOvertimeAvailability = overtimeAvailabilityEnd.Days > overtimeAvailabilityStart.Days ? new TimeSpan(23, 59, 59) : overtimeAvailabilityEnd;
			}

			return lateEnd > lateEndOvertimeAvailability ? lateEnd : lateEndOvertimeAvailability;
		}

		private TimePeriod getMinMaxTime(Dictionary<DateOnly, ScheduleDaysAndProjection> scheduleDaysAndProjections, DateOnlyPeriod week, DateOnly firstDayOfWeek)
		{
			var earliest = scheduleDaysAndProjections.Min(x => minFunction(x.Value, week, firstDayOfWeek));
			var latest = scheduleDaysAndProjections.Max(x => maxfunction(x.Value, firstDayOfWeek));

			var margin = TimeSpan.FromMinutes(15);

			var early = earliest;
			var late = latest;

			if (early > late)
			{
				early = latest;
				late = earliest;
			}

			early = early.Ticks > TimeSpan.Zero.Add(margin).Ticks ? early.Subtract(margin) : TimeSpan.Zero;
			late = late.Ticks < new TimeSpan(23, 59, 59).Subtract(margin).Ticks ? late.Add(margin) : new TimeSpan(23, 59, 59);

			var minMaxTime = new TimePeriod(early, late);
			return minMaxTime;
		}
	}
}