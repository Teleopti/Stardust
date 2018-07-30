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
using Teleopti.Interfaces.Domain;

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
			var periodStartDate = period.StartDate;

			var periodWithPreviousDay = new DateOnlyPeriod(periodStartDate.AddDays(-1), period.EndDate);
			var scheduleDays = _scheduleProvider.GetScheduleForPeriod(periodWithPreviousDay).ToList();

			var personRequestPeriods = _personRequestProvider.RetrieveRequestPeriodsForLoggedOnUser(period);
			var requestProbability = _absenceRequestProbabilityProvider.GetAbsenceRequestProbabilityForPeriod(period);
			var seatBookings = _seatBookingProvider.GetSeatBookingsForScheduleDays(scheduleDays);
			var scheduleDaysAndProjections = scheduleDays.ToDictionary(day => day.DateOnlyAsPeriod.DateOnly,
				day => new ScheduleDaysAndProjection
				{
					ScheduleDay = day,
					Projection = getProjection(day)
				});
			var minMaxTime = getMinMaxTime(scheduleDaysAndProjections, period);

			var scheduleDayDomainData = new DayScheduleDomainData
			{
				Date = date,
				IsCurrentDay = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _userTimeZone.TimeZone()).Date == date.Date,
				MinMaxTime = minMaxTime,
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

			if (scheduleDaysAndProjections.ContainsKey(date.AddDays(-1)))
			{
				var yesterday = scheduleDaysAndProjections[date.AddDays(-1)];
				var scheduleYesterday = yesterday.ScheduleDay;
				scheduleDayDomainData.ProjectionYesterday = yesterday.Projection;
				scheduleDayDomainData.OvertimeAvailabilityYesterday = scheduleYesterday.OvertimeAvailablityCollection() == null
					? null
					: scheduleYesterday.OvertimeAvailablityCollection().FirstOrDefault();
			}

			if (personRequestPeriods != null)
			{
				scheduleDayDomainData.PersonRequestCount = personRequestPeriods
					.Where(r => TimeZoneInfo.ConvertTimeFromUtc(r.StartDateTime, _userTimeZone.TimeZone())
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

			scheduleDayDomainData.AsmEnabled = _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger) && isAsmLicenseAvailable();
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
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequests);
			scheduleDayDomainData.PersonAccountPermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewPersonalAccount);
			scheduleDayDomainData.ViewPossibilityPermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewStaffingInfo);

			return scheduleDayDomainData;
		}

		private IVisualLayerCollection getProjection(IScheduleDay day)
		{
			return _projectionProvider.Projection(day);
		}

		private bool isAsmLicenseAvailable()
		{
			return _licenseAvailability.IsLicenseEnabled(DefinedLicenseOptionPaths.TeleoptiCccAgentScheduleMessenger);
		}

		private TimeSpan minFunction(ScheduleDaysAndProjection scheduleDayData, DateOnlyPeriod period)
		{
			var scheduleDay = scheduleDayData.ScheduleDay;
			var projection = scheduleDayData.Projection;
			var schedulePeriods = projection.Period();
			var earlyStart = new TimeSpan(23, 59, 59);
			var periodStartDate = period.StartDate;

			if (schedulePeriods != null && projection.HasLayers)
			{
				var userTimeZone = _userTimeZone.TimeZone();
				var startTime = schedulePeriods.Value.TimePeriod(userTimeZone).StartTime;
				var endTime = schedulePeriods.Value.TimePeriod(userTimeZone).EndTime;
				var localEndDate = new DateOnly(schedulePeriods.Value.EndDateTimeLocal(userTimeZone).Date);
				if (endTime.Days > startTime.Days && endTime > TimeSpan.FromDays(1) && period.Contains(localEndDate))
				{
					earlyStart = TimeSpan.Zero;
				}
				else if (scheduleDay.DateOnlyAsPeriod.DateOnly != periodStartDate.AddDays(-1))
				{
					earlyStart = startTime;
				}
			}

			var overtimeAvailabilityCollection = scheduleDay.OvertimeAvailablityCollection();
			if (overtimeAvailabilityCollection == null)
			{
				return earlyStart;
			}

			var overtimeAvailability = overtimeAvailabilityCollection.FirstOrDefault();
			if (overtimeAvailability == null)
			{
				return earlyStart;
			}

			var earlyStartOvertimeAvailability = new TimeSpan(23, 59, 59);
			var overtimeAvailabilityStart = overtimeAvailability.StartTime.Value;
			var overtimeAvailabilityEnd = overtimeAvailability.EndTime.Value;
			if (overtimeAvailabilityEnd.Days > overtimeAvailabilityStart.Days &&
				overtimeAvailabilityEnd > TimeSpan.FromDays(1) &&
				period.Contains(scheduleDay.DateOnlyAsPeriod.DateOnly.AddDays(1)))
			{
				earlyStartOvertimeAvailability = TimeSpan.Zero;
			}
			else if (scheduleDay.DateOnlyAsPeriod.DateOnly != periodStartDate.AddDays(-1))
			{
				earlyStartOvertimeAvailability = overtimeAvailabilityStart;
			}

			return earlyStart < earlyStartOvertimeAvailability ? earlyStart : earlyStartOvertimeAvailability;
		}

		private TimeSpan maxfunction(ScheduleDaysAndProjection scheduleDayData, DateOnly periodStartDate)
		{
			var scheduleDay = scheduleDayData.ScheduleDay;
			var projection = scheduleDayData.Projection;
			var period = projection.Period();
			var lateEnd = TimeSpan.Zero;
			if (period != null && projection.HasLayers)
			{
				var userTimeZone = _userTimeZone.TimeZone();
				var endTime = period.Value.TimePeriod(userTimeZone).EndTime;

				//for the day before current week, only if end time crosses midnihgt, 
				//then it is a valid end time to be carried over to first week day (endTime.Days == 1)
				if (scheduleDay.DateOnlyAsPeriod.DateOnly == periodStartDate.AddDays(-1) && endTime.Days == 1)
				{
					lateEnd = endTime - TimeSpan.FromDays(1);
				}
				else if (scheduleDay.DateOnlyAsPeriod.DateOnly != periodStartDate.AddDays(-1)) //for the days of current week
				{
					//if end time cross midnight and not allow timeline to cross night, then max. time is used, otherwise use the end time as it is
					lateEnd = endTime;
				}
			}

			var overtimeAvailabilityCollection = scheduleDay.OvertimeAvailablityCollection();
			if (overtimeAvailabilityCollection == null)
			{
				return lateEnd;
			}

			var overtimeAvailability = overtimeAvailabilityCollection.FirstOrDefault();
			if (overtimeAvailability == null)
			{
				return lateEnd;
			}

			var lateEndOvertimeAvailability = TimeSpan.Zero;
			var overtimeAvailabilityStart = overtimeAvailability.StartTime.Value;
			var overtimeAvailabilityEnd = overtimeAvailability.EndTime.Value;

			//for the day before current week, only if end time of OT Availability crosses midnight (ie., overtimeAvailabilityEnd.Days == 1), 
			//then it is a valid end time to be carried over to the first week day 
			if (scheduleDay.DateOnlyAsPeriod.DateOnly == periodStartDate.AddDays(-1) && overtimeAvailabilityEnd.Days == 1)
			{
				lateEndOvertimeAvailability = overtimeAvailabilityEnd - TimeSpan.FromDays(1);
			}
			else if (scheduleDay.DateOnlyAsPeriod.DateOnly != periodStartDate.AddDays(-1)) //for the days of current week
			{
				//if end time of OT Availability crosses midnight, then max. time is of course used, otherwise use its end time as it is
				lateEndOvertimeAvailability = overtimeAvailabilityEnd.Days > overtimeAvailabilityStart.Days
					? new TimeSpan(23, 59, 59)
					: overtimeAvailabilityEnd;
			}

			return lateEnd > lateEndOvertimeAvailability ? lateEnd : lateEndOvertimeAvailability;
		}

		private TimePeriod getMinMaxTime(Dictionary<DateOnly, ScheduleDaysAndProjection> scheduleDaysAndProjections,
			DateOnlyPeriod period)
		{
			var earliest = scheduleDaysAndProjections.Min(x => minFunction(x.Value, period));
			var latest = scheduleDaysAndProjections.Max(x => maxfunction(x.Value, period.StartDate));
			var early = earliest;
			var late = latest;
			if (early > late)
			{
				early = latest;
				late = earliest;
			}

			var margin = TimeSpan.FromMinutes(ScheduleConsts.TimelineMarginInMinute);
			early = early.Ticks > TimeSpan.Zero.Add(margin).Ticks ? early.Subtract(margin) : TimeSpan.Zero;

			late = late.Add(margin);

			var minMaxTime = new TimePeriod(early, late);
			return minMaxTime;
		}

		internal class ScheduleDaysAndProjection
		{
			public IScheduleDay ScheduleDay { get; set; }
			public IVisualLayerCollection Projection { get; set; }
		}
	}
}