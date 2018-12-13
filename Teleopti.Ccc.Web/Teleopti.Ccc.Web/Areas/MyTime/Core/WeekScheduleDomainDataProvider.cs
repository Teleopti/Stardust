using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Ccc.Web.Core.Extensions;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	public class WeekScheduleDomainDataProvider : IWeekScheduleDomainDataProvider
	{
		private readonly IScheduleProvider _scheduleProvider;
		private readonly IProjectionProvider _projectionProvider;
		private readonly IPersonRequestProvider _personRequestProvider;
		private readonly ISeatOccupancyProvider _seatBookingProvider;
		private readonly IUserTimeZone _userTimeZone;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPermissionProvider _permissionProvider;
		private readonly INow _now;
		private readonly ILicenseAvailability _licenseAvailability;
		private readonly IAbsenceRequestProbabilityProvider _absenceRequestProbabilityProvider;

		public WeekScheduleDomainDataProvider(IScheduleProvider scheduleProvider,
			IProjectionProvider projectionProvider,
			IPersonRequestProvider personRequestProvider,
			ISeatOccupancyProvider seatBookingProvider,
			IUserTimeZone userTimeZone,
			ILoggedOnUser loggedOnUser,
			IPermissionProvider permissionProvider,
			INow now,
			IAbsenceRequestProbabilityProvider absenceRequestProbabilityProvider, ILicenseAvailability licenseAvailability)
		{
			_scheduleProvider = scheduleProvider;
			_projectionProvider = projectionProvider;
			_personRequestProvider = personRequestProvider;
			_seatBookingProvider = seatBookingProvider;
			_userTimeZone = userTimeZone;
			_loggedOnUser = loggedOnUser;
			_permissionProvider = permissionProvider;
			_now = now;
			_absenceRequestProbabilityProvider = absenceRequestProbabilityProvider;
			_licenseAvailability = licenseAvailability;
		}

		internal class ScheduleDaysAndProjection
		{
			public IScheduleDay ScheduleDay { get; set; }
			public IVisualLayerCollection Projection { get; set; }
		}

		public WeekScheduleDomainData GetWeekSchedule(DateOnly date)
		{
			var firstDayOfWeek = DateHelper.GetFirstDateInWeek(date, DateTimeFormatExtensions.FirstDayOfWeek);
			var week = new DateOnlyPeriod(firstDayOfWeek, firstDayOfWeek.AddDays(6));
			return getScheduleDomainData(date, week);
		}

		private WeekScheduleDomainData getScheduleDomainData(DateOnly date, DateOnlyPeriod period)
		{
			var periodStartDate = period.StartDate;

			var timeZone = _userTimeZone.TimeZone();
			var periodWithPreviousDay = new DateOnlyPeriod(periodStartDate.AddDays(-1), period.EndDate);
			var scheduleDays = _scheduleProvider.GetScheduleForPeriod(periodWithPreviousDay,
				new ScheduleDictionaryLoadOptions(true, true, true) {LoadAgentDayScheduleTags = false}).ToArray();

			var personRequestPeriods = _personRequestProvider.RetrieveRequestPeriodsForLoggedOnUser(period).ToLookup(r => r.StartDateTimeLocal(timeZone).ToDateOnly());
			var requestProbability = _absenceRequestProbabilityProvider.GetAbsenceRequestProbabilityForPeriod(period).ToLookup(r => r.Date);
			var seatBookings = _seatBookingProvider.GetSeatBookingsForScheduleDays(periodWithPreviousDay, _loggedOnUser.CurrentUser()).ToLookup(b => b.BelongsToDate);
			var scheduleDaysAndProjections = scheduleDays.ToDictionary(day => day.DateOnlyAsPeriod.DateOnly,
				day => new ScheduleDaysAndProjection
				{
					ScheduleDay = day,
					Projection = _projectionProvider.Projection(day)
				});
			var minMaxTime = getMinMaxTime(scheduleDaysAndProjections, period);

			var days = periodStartDate.DateRange(period.DayCount()).Select(day =>
			{
				var scheduleDayDomainData = new WeekScheduleDayDomainData
				{
					Date = day,
					MinMaxTime = minMaxTime,
					ProbabilityClass = "",
					ProbabilityText = "",
				};

				if (scheduleDaysAndProjections.TryGetValue(day, out var theDay))
				{
					var scheduleDay = theDay.ScheduleDay;
					scheduleDayDomainData.PersonAssignment = scheduleDay.PersonAssignment();
					scheduleDayDomainData.SignificantPartForDisplay = scheduleDay.SignificantPartForDisplay();
					scheduleDayDomainData.ScheduleDay = scheduleDay;
					scheduleDayDomainData.Projection = theDay.Projection;
					scheduleDayDomainData.OvertimeAvailability =
						scheduleDay.OvertimeAvailablityCollection()?.FirstOrDefault();
				}

				if (scheduleDaysAndProjections.TryGetValue(day.AddDays(-1), out var yesterday))
				{
					var scheduleYesterday = yesterday.ScheduleDay;
					scheduleDayDomainData.ProjectionYesterday = yesterday.Projection;
					scheduleDayDomainData.OvertimeAvailabilityYesterday =
						scheduleYesterday.OvertimeAvailablityCollection()?.FirstOrDefault();
				}

				scheduleDayDomainData.PersonRequestCount = personRequestPeriods[day].Count();

				var absenceRequestProbability = requestProbability[day].FirstOrDefault();
				if (absenceRequestProbability != null)
				{
					scheduleDayDomainData.Availability = absenceRequestProbability.Availability;
					scheduleDayDomainData.ProbabilityClass = absenceRequestProbability.CssClass;
					scheduleDayDomainData.ProbabilityText = absenceRequestProbability.Text;
				}

				scheduleDayDomainData.SeatBookingInformation = seatBookings[day].ToArray();

				return scheduleDayDomainData;
			}).ToArray();
			
			var colorSource = new ScheduleColorSource
			{
				ScheduleDays = scheduleDays,
				Projections = (from d in days where d.Projection != null select d.Projection).ToArray()
			};

			var asmPermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger);
			var textRequestPermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.TextRequests);
			var overtimeAvailabilityPermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.OvertimeAvailabilityWeb);
			var absenceRequestPermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb);
			var overtimeRequestPermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.OvertimeRequestWeb);
			var absenceReportPermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AbsenceReport);
			var shiftTradeBulletinBoardPermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeBulletinBoard);
			var shiftExchangePermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeBulletinBoard);
			var personalAccountPermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewPersonalAccount);
			var viewPossibilityPermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewStaffingInfo);

			var isCurrentWeek = period.Contains(new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), timeZone)));

			return new WeekScheduleDomainData
			{
				Date = date,
				Days = days,
				ColorSource = colorSource,
				MinMaxTime = minMaxTime,
				AsmEnabled = asmPermission && isAsmLicenseAvailable(),
				TextRequestPermission = textRequestPermission,
				OvertimeAvailabilityPermission = overtimeAvailabilityPermission,
				AbsenceRequestPermission = absenceRequestPermission,
				OvertimeRequestPermission = overtimeRequestPermission,
				AbsenceReportPermission = absenceReportPermission,
				ShiftExchangePermission = shiftExchangePermission,
				ShiftTradeBulletinBoardPermission = shiftTradeBulletinBoardPermission,
				PersonAccountPermission = personalAccountPermission,
				ViewPossibilityPermission = viewPossibilityPermission,
				IsCurrentWeek = isCurrentWeek
			};
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
				var startTime = period.Value.TimePeriod(userTimeZone).StartTime;
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
					lateEnd = (endTime.Days > startTime.Days) ? new TimeSpan(23, 59, 59) : endTime;
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

			late = late.Ticks < new TimeSpan(23, 59, 59).Subtract(margin).Ticks ? late.Add(margin) : new TimeSpan(23, 59, 59);

			var minMaxTime = new TimePeriod(early, late);
			return minMaxTime;
		}
	}
}