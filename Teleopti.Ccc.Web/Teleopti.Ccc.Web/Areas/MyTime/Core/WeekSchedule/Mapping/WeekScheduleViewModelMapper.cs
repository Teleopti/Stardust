using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Ccc.Web.Core.Extensions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping
{
	public class WeekScheduleViewModelMapper
	{
		private readonly IPeriodSelectionViewModelFactory _periodSelectionViewModelFactory;
		private readonly IPeriodViewModelFactory _periodViewModelFactory;
		private readonly IHeaderViewModelFactory _headerViewModelFactory;
		private readonly IScheduleColorProvider _scheduleColorProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly INow _now;
		private readonly CommonViewModelMapper _commonMapper;
		private readonly OvertimeAvailabilityViewModelMapper _overtimeMapper;

		public WeekScheduleViewModelMapper(IPeriodSelectionViewModelFactory periodSelectionViewModelFactory,
			IPeriodViewModelFactory periodViewModelFactory,
			IHeaderViewModelFactory headerViewModelFactory,
			IScheduleColorProvider scheduleColorProvider,
			ILoggedOnUser loggedOnUser, INow now,
			CommonViewModelMapper commonMapper,
			OvertimeAvailabilityViewModelMapper overtimeMapper)
		{
			_periodSelectionViewModelFactory = periodSelectionViewModelFactory;
			_periodViewModelFactory = periodViewModelFactory;
			_headerViewModelFactory = headerViewModelFactory;
			_scheduleColorProvider = scheduleColorProvider;
			_loggedOnUser = loggedOnUser;
			_now = now;
			_commonMapper = commonMapper;
			_overtimeMapper = overtimeMapper;
		}

		public WeekScheduleViewModel Map(WeekScheduleDomainData s)
		{
			var currentUser = _loggedOnUser.CurrentUser();
			var timeZone = currentUser.PermissionInformation.DefaultTimeZone();

			var daylightSavingAdjustment = TimeZoneHelper.GetDaylightChanges(
				timeZone, _now.LocalDateTime().Year);
			var daylightModel = daylightSavingAdjustment != null
				? new DaylightSavingsTimeAdjustmentViewModel(daylightSavingAdjustment)
				: null;
			var firstDayOfWeek = DateHelper.GetFirstDateInWeek(s.Date, DateTimeFormatExtensions.FirstDayOfWeek);
			return new WeekScheduleViewModel
			{
				BaseUtcOffsetInMinutes = timeZone.BaseUtcOffset.TotalMinutes,
				DaylightSavingTimeAdjustment = daylightModel,
				CurrentWeekStartDate = firstDayOfWeek.ToFixedClientDateOnlyFormat(),
				CurrentWeekEndDate = firstDayOfWeek.AddDays(6).ToFixedClientDateOnlyFormat(),
				PeriodSelection = _periodSelectionViewModelFactory.CreateModel(s.Date),
				Styles = s.Days == null ? null : map(_scheduleColorProvider.GetColors(s.ColorSource)),
				TimeLine = createTimeLine(s.MinMaxTime).ToArray(),
				RequestPermission = map(s),
				ViewPossibilityPermission = s.ViewPossibilityPermission,
				DatePickerFormat = DateTimeFormatExtensions.LocalizedDateFormat,
				Days = days(s),
				AsmPermission = s.AsmPermission,
				IsCurrentWeek = s.IsCurrentWeek,
				CheckStaffingByIntraday = isCheckStaffingByIntraday(currentUser.WorkflowControlSet, timeZone),
				SiteOpenHourIntradayPeriod = getSiteOpenHourPeriod(_now.LocalDateOnly())
			};
		}

		public DayScheduleViewModel Map(DayScheduleDomainData s)
		{
			var currentUser = _loggedOnUser.CurrentUser();
			var timeZone = currentUser.PermissionInformation.DefaultTimeZone();

			var daylightSavingAdjustment = TimeZoneHelper.GetDaylightChanges(
				timeZone, _now.LocalDateTime().Year);
			var daylightModel = daylightSavingAdjustment != null
				? new DaylightSavingsTimeAdjustmentViewModel(daylightSavingAdjustment)
				: null;
			return new DayScheduleViewModel
			{
				Date = s.Date.ToFixedClientDateOnlyFormat(),
				DisplayDate = s.Date.Date.ToLocalizedDateFormat(),
				BaseUtcOffsetInMinutes = timeZone.BaseUtcOffset.TotalMinutes,
				DaylightSavingTimeAdjustment = daylightModel,
				TimeLine = createTimeLine(s.MinMaxTime).ToArray(),
				RequestPermission = map(s),
				ViewPossibilityPermission = s.ViewPossibilityPermission,
				DatePickerFormat = DateTimeFormatExtensions.LocalizedDateFormat,
				Schedule = createDayViewModel(s.ScheduleDay),
				AsmPermission = s.AsmPermission,
				IsToday = s.IsCurrentDay,
				CheckStaffingByIntraday = isCheckStaffingByIntraday(currentUser.WorkflowControlSet, timeZone),
				SiteOpenHourIntradayPeriod = getSiteOpenHourPeriod(s.Date)
			};
		}

		private static bool isCheckStaffingByIntraday(IWorkflowControlSet workflowControlSet,
			TimeZoneInfo timeZone)
		{
			if (workflowControlSet?.AbsenceRequestOpenPeriods == null || !workflowControlSet.AbsenceRequestOpenPeriods.Any())
			{
				return false;
			}
			return workflowControlSet.IsAbsenceRequestValidatorEnabled<StaffingThresholdWithShrinkageValidator>(timeZone) ||
					workflowControlSet.IsAbsenceRequestValidatorEnabled<StaffingThresholdValidator>(timeZone);
		}

		private IEnumerable<DayViewModel> days(WeekScheduleDomainData scheduleDomainData)
		{
			return scheduleDomainData?.Days?.Select(createDayViewModel).ToArray();
		}

		private DayViewModel createDayViewModel(WeekScheduleDayDomainData s)
		{
			var personAssignment = s.ScheduleDay?.PersonAssignment();
			var significantPartForDisplay = s.ScheduleDay?.SignificantPartForDisplay();
			return new DayViewModel
			{
				Date = s.Date.ToShortDateString(),
				FixedDate = s.Date.ToFixedClientDateOnlyFormat(),
				DayOfWeekNumber = (int) s.Date.DayOfWeek,
				Periods = projections(s).ToArray(),
				TextRequestCount =
					s.PersonRequests?.Count(
						r => r.Request is TextRequest || r.Request is AbsenceRequest || (r.Request is ShiftExchangeOffer)) ?? 0,
				ProbabilityClass = s.ProbabilityClass,
				ProbabilityText = s.ProbabilityText,
				State = s.Date == _now.LocalDateOnly() ? SpecialDateState.Today : 0,
				Header = _headerViewModelFactory.CreateModel(s.ScheduleDay),
				Note = s.ScheduleDay == null ? null : map(s.ScheduleDay.PublicNoteCollection()),
				SeatBookings = s.SeatBookingInformation,
				Summary = summary(s),
				HasOvertime = personAssignment != null && personAssignment.ShiftLayers.OfType<OvertimeShiftLayer>().Any(),
				IsFullDayAbsence = significantPartForDisplay == SchedulePartView.FullDayAbsence,
				IsDayOff = significantPartForDisplay == SchedulePartView.DayOff,
				OvertimeAvailabililty = overtimeAvailability(s),
				Availability = s.Availability,
				SiteOpenHourPeriod = getSiteOpenHourPeriod(s.Date)
			};
		}

		private NoteViewModel map(ReadOnlyCollection<IPublicNote> s)
		{
			var publicNote = s.FirstOrDefault();
			return new NoteViewModel
			{
				Message = publicNote != null ? publicNote.GetScheduleNote(new NoFormatting()) : string.Empty
			};
		}

		private RequestPermission map(BaseScheduleDomainData s)
		{
			return new RequestPermission
			{
				AbsenceReportPermission = s.AbsenceReportPermission,
				AbsenceRequestPermission = s.AbsenceRequestPermission,
				OvertimeAvailabilityPermission = s.OvertimeAvailabilityPermission,
				PersonAccountPermission = s.PersonAccountPermission,
				ShiftTradeBulletinBoardPermission = s.ShiftTradeBulletinBoardPermission,
				ShiftExchangePermission = s.ShiftExchangePermission,
				TextRequestPermission = s.TextRequestPermission
			};
		}

		private IEnumerable<StyleClassViewModel> map(IEnumerable<Color> colors)
		{
			return colors?.Select(_commonMapper.Map).ToArray();
		}

		private IEnumerable<TimeLineViewModel> createTimeLine(TimePeriod timelinePeriod)
		{
			var startTime = timelinePeriod.StartTime;
			var endTime = timelinePeriod.EndTime;

			var firstHour = startTime
				.Subtract(new TimeSpan(0, 0, startTime.Minutes, startTime.Seconds, startTime.Milliseconds))
				.Add(TimeSpan.FromHours(1));
			var lastHour = endTime
				.Subtract(new TimeSpan(0, 0, endTime.Minutes, endTime.Seconds, endTime.Milliseconds));
			var times = firstHour
				.TimeRange(lastHour, TimeSpan.FromHours(1))
				.Append(startTime)
				.Append(endTime)
				.Append(firstHour)
				.OrderBy(t => t)
				.Distinct();

			var diff = endTime - startTime;
			return times.Select(t => new TimeLineViewModel
			{
				Time = t,
				TimeLineDisplay = new DateTime().Add(t).ToLocalizedTimeFormat(),
				PositionPercentage = diff == TimeSpan.Zero ? 0 : (decimal) (t - startTime).Ticks / diff.Ticks
			});
		}

		private IEnumerable<PeriodViewModel> projections(WeekScheduleDayDomainData s)
		{
			var projectionList = new List<IVisualLayer>();
			if (s.ProjectionYesterday != null)
				projectionList.AddRange(s.ProjectionYesterday);
			if (s.Projection != null)
				projectionList.AddRange(s.Projection);
			var periodViewModelFactory = _periodViewModelFactory;
			var periodsViewModels = periodViewModelFactory.CreatePeriodViewModels(projectionList, s.MinMaxTime, s.Date.Date,
				s.ScheduleDay?.TimeZone) ?? new PeriodViewModel[0];
			var overtimeAvailabilityPeriodViewModels =
				periodViewModelFactory.CreateOvertimeAvailabilityPeriodViewModels(s.OvertimeAvailability,
					s.OvertimeAvailabilityYesterday, s.MinMaxTime) ?? new OvertimeAvailabilityPeriodViewModel[0];
			return periodsViewModels.Concat(overtimeAvailabilityPeriodViewModels).OrderBy(p => p.StartTime);
		}

		private OvertimeAvailabilityViewModel overtimeAvailability(WeekScheduleDayDomainData s)
		{
			if (s.OvertimeAvailability != null)
				return _overtimeMapper.Map(s.OvertimeAvailability);
			if (s.ScheduleDay?.SignificantPartForDisplay() == SchedulePartView.MainShift)
			{
				var personAssignment = s.ScheduleDay.PersonAssignment();
				return new OvertimeAvailabilityViewModel
				{
					HasOvertimeAvailability = false,
					DefaultStartTime = TimeHelper.TimeOfDayFromTimeSpan(
						personAssignment.Period.TimePeriod(s.ScheduleDay.TimeZone).EndTime, CultureInfo.CurrentCulture),
					DefaultEndTime = TimeHelper.TimeOfDayFromTimeSpan(
						personAssignment.Period.TimePeriod(s.ScheduleDay.TimeZone).EndTime.Add(TimeSpan.FromHours(1)),
						CultureInfo.CurrentCulture),
					DefaultEndTimeNextDay =
						personAssignment
							.Period.TimePeriod(s.ScheduleDay.TimeZone)
							.EndTime.Add(TimeSpan.FromHours(1))
							.Days > 0
				};
			}
			return new OvertimeAvailabilityViewModel
			{
				HasOvertimeAvailability = false,
				DefaultStartTime = TimeHelper.TimeOfDayFromTimeSpan(TimeSpan.FromHours(8)),
				DefaultEndTime = TimeHelper.TimeOfDayFromTimeSpan(TimeSpan.FromHours(17)),
				DefaultEndTimeNextDay = false
			};
		}

		private static PeriodViewModel summary(WeekScheduleDayDomainData s)
		{
			if (s.ScheduleDay != null)
			{
				var significantPart = s.ScheduleDay.SignificantPartForDisplay();

				if (significantPart == SchedulePartView.ContractDayOff)
				{
					var personAbsence = s.ScheduleDay.PersonAbsenceCollection()
					   .OrderBy(a => a.Layer.Payload.Priority)
					   .ThenByDescending(a => s.ScheduleDay.PersonAbsenceCollection().IndexOf(a))
					   .First();
					var periodViewModel = new FullDayAbsencePeriodViewModel
					{
						Title =
							personAbsence
								.Layer.Payload.Description.Name,
						Summary = TimeHelper.GetLongHourMinuteTimeString(s.Projection.ContractTime(), CultureInfo.CurrentUICulture),
						StyleClassName =
							personAbsence
								.Layer.Payload.DisplayColor.ToStyleClass(),
						Color =
							$"rgb({personAbsence.Layer.Payload.DisplayColor.R},{personAbsence.Layer.Payload.DisplayColor.G},{personAbsence.Layer.Payload.DisplayColor.B})"
					};

					periodViewModel.StyleClassName += " " + StyleClasses.Striped;
					return periodViewModel;
				}
				if (significantPart == SchedulePartView.DayOff)
					return new PersonDayOffPeriodViewModel
					{
						Title = s.ScheduleDay?.PersonAssignment()?.DayOff()?.Description.Name,
						StyleClassName = StyleClasses.DayOff + " " + StyleClasses.Striped
					};
				if (significantPart == SchedulePartView.MainShift)
				{
					var personAssignment = s.ScheduleDay?.PersonAssignment();
					return new PersonAssignmentPeriodViewModel
					{
						Title = personAssignment?.ShiftCategory?.Description.Name,
						Summary = TimeHelper.GetLongHourMinuteTimeString(s.Projection.ContractTime(), CultureInfo.CurrentUICulture),
						TimeSpan =
							personAssignment?
								.PeriodExcludingPersonalActivity()
								.TimePeriod(s.ScheduleDay?.TimeZone)
								.ToShortTimeString(),
						StyleClassName = personAssignment?.ShiftCategory?.DisplayColor.ToStyleClass(),
						Color =
							$"rgb({personAssignment?.ShiftCategory?.DisplayColor.R},{personAssignment?.ShiftCategory?.DisplayColor.G},{personAssignment?.ShiftCategory?.DisplayColor.B})"
					};
				}
				if (significantPart == SchedulePartView.FullDayAbsence || significantPart == SchedulePartView.ContractDayOff)
				{
					var personAbsence = s.ScheduleDay.PersonAbsenceCollection()
						.OrderBy(a => a.Layer.Payload.Priority)
						.ThenByDescending(a => s.ScheduleDay.PersonAbsenceCollection().IndexOf(a))
						.First();
					return new FullDayAbsencePeriodViewModel
					{
						Title =
							personAbsence
								.Layer.Payload.Description.Name,
						Summary = TimeHelper.GetLongHourMinuteTimeString(s.Projection.ContractTime(), CultureInfo.CurrentUICulture),
						StyleClassName =
							personAbsence
								.Layer.Payload.DisplayColor.ToStyleClass(),
						Color =
							$"rgb({personAbsence.Layer.Payload.DisplayColor.R},{personAbsence.Layer.Payload.DisplayColor.G},{personAbsence.Layer.Payload.DisplayColor.B})",
					};
				}
			}
			return new PeriodViewModel();
		}

		private TimePeriod? getSiteOpenHourPeriod(DateOnly date)
		{
			var siteOpenHour = _loggedOnUser.CurrentUser().SiteOpenHour(date);
			if (siteOpenHour == null || siteOpenHour.IsClosed)
			{
				return null;
			}
			return siteOpenHour.TimePeriod;
		}
	}
}