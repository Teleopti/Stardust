﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Requests;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
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
		private readonly IRequestsViewModelFactory _requestsViewModelFactory;
		private readonly ISiteOpenHourProvider _siteOpenHourProvider;
		private readonly IScheduledSkillOpenHourProvider _scheduledSkillOpenHourProvider;
		private readonly ILicenseAvailability _licenseAvailability;
		private readonly IToggleManager _toggleManager;
		private readonly IStaffingDataAvailablePeriodProvider _staffingDataAvailablePeriodProvider;

		public WeekScheduleViewModelMapper(IPeriodSelectionViewModelFactory periodSelectionViewModelFactory,
			IPeriodViewModelFactory periodViewModelFactory,
			IHeaderViewModelFactory headerViewModelFactory,
			IScheduleColorProvider scheduleColorProvider,
			ILoggedOnUser loggedOnUser, INow now,
			CommonViewModelMapper commonMapper,
			OvertimeAvailabilityViewModelMapper overtimeMapper,
			IRequestsViewModelFactory requestsViewModelFactory,
			ISiteOpenHourProvider siteOpenHourProvider,
			IScheduledSkillOpenHourProvider scheduledSkillOpenHourProvider, ILicenseAvailability licenseAvailability,
			IToggleManager toggleManager, 
			IStaffingDataAvailablePeriodProvider staffingDataAvailablePeriodProvider)
		{
			_periodSelectionViewModelFactory = periodSelectionViewModelFactory;
			_periodViewModelFactory = periodViewModelFactory;
			_headerViewModelFactory = headerViewModelFactory;
			_scheduleColorProvider = scheduleColorProvider;
			_loggedOnUser = loggedOnUser;
			_now = now;
			_commonMapper = commonMapper;
			_overtimeMapper = overtimeMapper;
			_requestsViewModelFactory = requestsViewModelFactory;
			_siteOpenHourProvider = siteOpenHourProvider;
			_scheduledSkillOpenHourProvider = scheduledSkillOpenHourProvider;
			_licenseAvailability = licenseAvailability;
			_toggleManager = toggleManager;
			_staffingDataAvailablePeriodProvider = staffingDataAvailablePeriodProvider;
		}

		public WeekScheduleViewModel Map(WeekScheduleDomainData s, bool loadOpenHourPeriod = false)
		{
			var currentUser = _loggedOnUser.CurrentUser();
			var timeZone = currentUser.PermissionInformation.DefaultTimeZone();

			var daylightSavingAdjustment = TimeZoneHelper.GetDaylightChanges(
				timeZone, _now.ServerDateTime_DontUse().Year);
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
				TimeLine = createTimeLine(s.MinMaxTime, s.Date).ToArray(),
				RequestPermission = map(s),
				ViewPossibilityPermission = s.ViewPossibilityPermission,
				DatePickerFormat = DateTimeFormatExtensions.LocalizedDateFormat,
				Days = days(s, loadOpenHourPeriod),
				AsmEnabled = s.AsmEnabled,
				IsCurrentWeek = s.IsCurrentWeek,
				CheckStaffingByIntraday = isCheckStaffingByIntradayForWeek(currentUser.WorkflowControlSet, s.Date),
				AbsenceProbabilityEnabled = currentUser.WorkflowControlSet?.AbsenceProbabilityEnabled ?? false,
				OvertimeProbabilityEnabled = isOvertimeProbabilityEnabled(s.Date, true),
				StaffingInfoAvailableDays = StaffingInfoAvailableDaysProvider.GetDays(_toggleManager) + 1
			};
		}

		public DayScheduleViewModel Map(DayScheduleDomainData s, bool loadOpenHourPeriod = false)
		{
			var currentUser = _loggedOnUser.CurrentUser();
			var timeZone = currentUser.PermissionInformation.DefaultTimeZone();

			var daylightSavingAdjustment = TimeZoneHelper.GetDaylightChanges(
				timeZone, _now.ServerDateTime_DontUse().Year);
			var daylightModel = daylightSavingAdjustment != null
				? new DaylightSavingsTimeAdjustmentViewModel(daylightSavingAdjustment)
				{
					EnteringDST = s.Date.Date.Equals(TimeZoneHelper.ConvertFromUtc(daylightSavingAdjustment.Start, timeZone).Date),
					LocalDSTStartTimeInMinutes = (int)TimeZoneHelper.ConvertFromUtc(daylightSavingAdjustment.Start, timeZone).TimeOfDay.TotalMinutes
				}
				: null;

			var viewModel = new DayScheduleViewModel
			{
				Date = s.Date.ToFixedClientDateOnlyFormat(),
				BaseUtcOffsetInMinutes = timeZone.BaseUtcOffset.TotalMinutes,
				DaylightSavingTimeAdjustment = daylightModel,
				TimeLine = createTimeLine(s.MinMaxTime, s.Date, daylightSavingAdjustment, timeZone),
				RequestPermission = mapDaySchedulePermission(s),
				ViewPossibilityPermission = s.ViewPossibilityPermission,
				DatePickerFormat = DateTimeFormatExtensions.LocalizedDateFormat,
				Schedule = createDayViewModel(s.ScheduleDay, loadOpenHourPeriod),
				AsmEnabled = s.AsmEnabled,
				IsToday = s.IsCurrentDay,
				CheckStaffingByIntraday = isCheckStaffingByIntradayForDay(currentUser.WorkflowControlSet, s.Date),
				AbsenceProbabilityEnabled = currentUser.WorkflowControlSet?.AbsenceProbabilityEnabled ?? false,
				OvertimeProbabilityEnabled = isOvertimeProbabilityEnabled(s.Date, false),
				UnReadMessageCount = s.UnReadMessageCount,
				ShiftTradeRequestSetting = _requestsViewModelFactory.CreateShiftTradePeriodViewModel(),
				StaffingInfoAvailableDays = StaffingInfoAvailableDaysProvider.GetDays(_toggleManager) + 1
			};
			viewModel.Schedule.Periods = projections(s.ScheduleDay, false);

			return viewModel;
		}

		private bool isOvertimeProbabilityEnabled(DateOnly date, bool forThisWeek)
		{
			var currentUser = _loggedOnUser.CurrentUser();
			var overtimeProbabilityEnabled = currentUser.WorkflowControlSet?.OvertimeProbabilityEnabled != null
											 && currentUser.WorkflowControlSet.OvertimeProbabilityEnabled
											 && isOvertimeProbabilityLicenseAvailable();
			if (!overtimeProbabilityEnabled)
				return false;

			var isStaffingDataAvailable = _staffingDataAvailablePeriodProvider.GetPeriodForOvertime(date, forThisWeek).HasValue;
			return isStaffingDataAvailable;
		}

		private bool isOvertimeProbabilityLicenseAvailable()
		{
			return _licenseAvailability.IsLicenseEnabled(DefinedLicenseOptionPaths.TeleoptiCccOvertimeAvailability)
				   || _licenseAvailability.IsLicenseEnabled(DefinedLicenseOptionPaths.TeleoptiWfmOvertimeRequests);
		}

		private bool isCheckStaffingByIntradayForWeek(IWorkflowControlSet workflowControlSet, DateOnly showForDate)
		{
			if (workflowControlSet?.AbsenceRequestOpenPeriods == null || !workflowControlSet.AbsenceRequestOpenPeriods.Any())
			{
				return false;
			}
			var weekPeriod = DateHelper.GetWeekPeriod(showForDate, CultureInfo.CurrentCulture);
			if (weekPeriod.StartDate < _now.ServerDate_DontUse() && _now.ServerDate_DontUse() < weekPeriod.EndDate)
			{
				weekPeriod = new DateOnlyPeriod(_now.ServerDate_DontUse(), weekPeriod.EndDate);
			}

			var days = weekPeriod.DayCollection();

			return days.Any(day => workflowControlSet.IsAbsenceRequestCheckStaffingByIntraday(_now.ServerDate_DontUse(), day));
		}

		private bool isCheckStaffingByIntradayForDay(IWorkflowControlSet workflowControlSet, DateOnly showForDate)
		{
			if (workflowControlSet?.AbsenceRequestOpenPeriods == null || !workflowControlSet.AbsenceRequestOpenPeriods.Any())
			{
				return false;
			}
			return showForDate >= _now.ServerDate_DontUse() &&
				   workflowControlSet.IsAbsenceRequestCheckStaffingByIntraday(_now.ServerDate_DontUse(), showForDate);
		}

		private IEnumerable<DayViewModel> days(WeekScheduleDomainData scheduleDomainData, bool loadOpenHourPeriod = false)
		{
			return scheduleDomainData?.Days?.Select(s =>
			{
				var viewModel = createDayViewModel(s, loadOpenHourPeriod);
				viewModel.Periods = projections(s, true);
				return viewModel;
			}).ToArray();
		}

		private DayViewModel createDayViewModel(WeekScheduleDayDomainData s, bool loadOpenHourPeriod = false)
		{
			var personAssignment = s.ScheduleDay?.PersonAssignment();
			var significantPartForDisplay = s.ScheduleDay?.SignificantPartForDisplay();
			var dayViewModel = new DayViewModel
			{
				Date = s.Date.ToShortDateString(),
				FixedDate = s.Date.ToFixedClientDateOnlyFormat(),
				DayOfWeekNumber = (int)s.Date.DayOfWeek,
				RequestsCount = s.PersonRequestCount,
				ProbabilityClass = s.ProbabilityClass,
				ProbabilityText = s.ProbabilityText,
				State = s.Date == new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone())) ? SpecialDateState.Today : 0,
				Header = _headerViewModelFactory.CreateModel(s.ScheduleDay),
				Note = s.ScheduleDay == null ? null : map(s.ScheduleDay.PublicNoteCollection()),
				SeatBookings = s.SeatBookingInformation,
				Summary = summary(s),
				HasOvertime = personAssignment != null && personAssignment.ShiftLayers.OfType<OvertimeShiftLayer>().Any(),
				HasMainShift = personAssignment != null && personAssignment.ShiftLayers.OfType<MainShiftLayer>().Any(),
				IsFullDayAbsence = significantPartForDisplay == SchedulePartView.FullDayAbsence,
				IsDayOff = significantPartForDisplay == SchedulePartView.DayOff,
				OvertimeAvailabililty = overtimeAvailability(s),
				Availability = s.Availability,
				OpenHourPeriod = loadOpenHourPeriod ? getOpenHourPeriod(s) : null
			};
			dayViewModel.HasNotScheduled = dayViewModel.Summary.Title == Resources.NotScheduled;

			return dayViewModel;
		}

		private NoteViewModel map(IPublicNote[] s)
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
				OvertimeRequestPermission = s.OvertimeRequestPermission,
				OvertimeAvailabilityPermission = s.OvertimeAvailabilityPermission,
				PersonAccountPermission = s.PersonAccountPermission,
				ShiftTradeBulletinBoardPermission = s.ShiftTradeBulletinBoardPermission,
				ShiftExchangePermission = s.ShiftExchangePermission,
				TextRequestPermission = s.TextRequestPermission
			};
		}

		private RequestPermission mapDaySchedulePermission(DayScheduleDomainData s)
		{
			var permission = map(s);
			permission.ShiftTradeRequestPermission = s.ShiftTradeRequestPermission;
			return permission;
		}

		private IEnumerable<StyleClassViewModel> map(IEnumerable<Color> colors)
		{
			return colors?.Select(_commonMapper.Map).ToArray();
		}

		private IEnumerable<TimeLineViewModel> createTimeLine(TimePeriod timelinePeriod, DateOnly date, DaylightTime daylightTime = null, TimeZoneInfo timezone = null)
		{
			timelinePeriod = adjustMinEndTime(timelinePeriod);
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
				.Where(t => daylightTime == null || fiterByDayLightTime(t, date, daylightTime, timezone))
				.OrderBy(t => t)
				.Distinct()
				.ToList();

			var isOnEnteringDstDay = isEnteringDST(date, daylightTime, timezone);
			var localDayLightTimeStart = new DateTime();

			var diff = endTime - startTime;
			if (isOnEnteringDstDay)
			{
				diff = getAjustedTimeSpanByDayLightTime(diff, daylightTime);
				localDayLightTimeStart = TimeZoneHelper.ConvertFromUtc(daylightTime.Start, timezone);
			}

			var timesCount = times.Count;
			for (var i = 0; i < timesCount; i++)
			{
				var time = times[i];
				if (isOnEnteringDstDay && isTimeSpanInDSTPeriod(time, localDayLightTimeStart))
				{
					time = getAjustedTimeSpanByDayLightTime(time, daylightTime);
				}

				yield return new TimeLineViewModel
				{
					Time = times[i],
					TimeLineDisplay = new DateTime().Add(times[i]).ToLocalizedTimeFormat(),
					PositionPercentage = diff == TimeSpan.Zero ? 0 : (decimal)(time - startTime).Ticks / diff.Ticks
				};
			}
		}

		private bool isEnteringDST(DateOnly localDate, DaylightTime daylightTime, TimeZoneInfo timezone)
		{
			if (daylightTime == null)
				return false;

			return TimeZoneHelper.ConvertFromUtc(daylightTime.Start, timezone).Date
					   .CompareTo(localDate.Date) == 0;
		}

		private bool isTimeSpanInDSTPeriod(TimeSpan timeSpan, DateTime localDayLightTimeStart)
		{
			return timeSpan.CompareTo(localDayLightTimeStart.TimeOfDay) >= 0;
		}

		private TimeSpan getAjustedTimeSpanByDayLightTime(TimeSpan timeSpan, DaylightTime daylightTime)
		{
			return timeSpan.Subtract(daylightTime.Delta);
		}

		private bool fiterByDayLightTime(TimeSpan timeSpan, DateOnly date, DaylightTime daylightTime, TimeZoneInfo timezone)
		{
			var localDayLightTimeStart = TimeZoneHelper.ConvertFromUtc(daylightTime.Start, timezone);

			return date.Date.Add(timeSpan).CompareTo(localDayLightTimeStart.AddMinutes(-daylightTime.Delta.TotalMinutes)) < 0
				   || date.Date.Add(timeSpan).CompareTo(localDayLightTimeStart) >= 0;
		}

		private IEnumerable<PeriodViewModel> projections(WeekScheduleDayDomainData s, bool forWeek)
		{
			var projectionList = new List<IVisualLayer>();
			if (s.ProjectionYesterday != null)
			{
				projectionList.AddRange(s.ProjectionYesterday);
			}

			if (s.Projection != null)
			{
				projectionList.AddRange(s.Projection);
			}

			var periodViewModelFactory = _periodViewModelFactory;
			var minMaxTime = adjustMinEndTime(s.MinMaxTime);

			IEnumerable<PeriodViewModel> periodsViewModels;
			if (forWeek)
				periodsViewModels = periodViewModelFactory.CreatePeriodViewModelsForWeek(projectionList, minMaxTime, s.Date,
					s.ScheduleDay?.TimeZone);
			else
				periodsViewModels = periodViewModelFactory.CreatePeriodViewModelsForDay(projectionList, minMaxTime, s.Date,
					s.ScheduleDay?.TimeZone);

			periodsViewModels = periodsViewModels ?? new PeriodViewModel[0];

			var overtimeAvailabilityPeriodViewModels =
				periodViewModelFactory.CreateOvertimeAvailabilityPeriodViewModels(s.OvertimeAvailability,
					s.OvertimeAvailabilityYesterday, minMaxTime) ?? new OvertimeAvailabilityPeriodViewModel[0];
			return periodsViewModels.Concat(overtimeAvailabilityPeriodViewModels).OrderBy(p => p.StartTime);
		}

		private TimePeriod adjustMinEndTime(TimePeriod period)
		{
			if (period.EndTime < TimeSpan.FromHours(1))
			{
				return new TimePeriod(period.StartTime, TimeSpan.FromHours(1));
			}
			return period;
		}

		private OvertimeAvailabilityViewModel overtimeAvailability(WeekScheduleDayDomainData s)
		{
			if (s.OvertimeAvailability != null)
			{
				return _overtimeMapper.Map(s.OvertimeAvailability);
			}

			if (s.ScheduleDay?.SignificantPartForDisplay() != SchedulePartView.MainShift)
			{
				return new OvertimeAvailabilityViewModel
				{
					HasOvertimeAvailability = false,
					DefaultStartTime = TimeHelper.TimeOfDayFromTimeSpan(TimeSpan.FromHours(8)),
					DefaultEndTime = TimeHelper.TimeOfDayFromTimeSpan(TimeSpan.FromHours(17)),
					DefaultEndTimeNextDay = false
				};
			}

			var personAssignment = s.ScheduleDay.PersonAssignment();
			var endTime = personAssignment.Period.TimePeriod(s.ScheduleDay.TimeZone).EndTime;
			var endTimeTomorrow = endTime.Add(TimeSpan.FromHours(1));
			return new OvertimeAvailabilityViewModel
			{
				HasOvertimeAvailability = false,
				DefaultStartTime = TimeHelper.TimeOfDayFromTimeSpan(endTime, CultureInfo.CurrentCulture),
				DefaultEndTime = TimeHelper.TimeOfDayFromTimeSpan(endTimeTomorrow, CultureInfo.CurrentCulture),
				DefaultEndTimeNextDay = endTimeTomorrow.Days > 0
			};
		}

		private static PeriodViewModel summary(WeekScheduleDayDomainData s)
		{
			if (s.ScheduleDay == null)
			{
				return new PeriodViewModel
				{
					Title = Resources.NotScheduled
				};
			}

			var significantPart = s.ScheduleDay.SignificantPartForDisplay();

			switch (significantPart)
			{
				case SchedulePartView.ContractDayOff:
					{
						var personAbsence = s.ScheduleDay.PersonAbsenceCollection()
							.OrderBy(a => a.Layer.Payload.Priority)
							.ThenByDescending(a => s.ScheduleDay.PersonAbsenceCollection().IndexOf(a))
							.First();
						var periodViewModel = new FullDayAbsencePeriodViewModel
						{
							Title = personAbsence.Layer.Payload.Description.Name,
							Summary = toFormattedTimeSpan(s.Projection.ContractTime()),
							StyleClassName = personAbsence.Layer.Payload.DisplayColor.ToStyleClass(),
							Color = toRgbColor(personAbsence.Layer.Payload.DisplayColor)
						};

						periodViewModel.StyleClassName += " " + StyleClasses.Striped;
						return periodViewModel;
					}
				case SchedulePartView.DayOff:
					{
						return new PersonDayOffPeriodViewModel
						{
							Title = s.ScheduleDay?.PersonAssignment()?.DayOff()?.Description.Name,
							StyleClassName = StyleClasses.DayOff + " " + StyleClasses.Striped
						};
					}
				case SchedulePartView.MainShift:
					{
						var personAssignment = s.ScheduleDay?.PersonAssignment();
						var shiftCategory = personAssignment?.ShiftCategory;
						return new PersonAssignmentPeriodViewModel
						{
							Title = shiftCategory?.Description.Name,
							Summary = toFormattedTimeSpan(s.Projection.ContractTime()),
							TimeSpan = personAssignment?.PeriodExcludingPersonalActivity()
								.TimePeriod(s.ScheduleDay?.TimeZone)
								.ToShortTimeString(),
							StyleClassName = shiftCategory?.DisplayColor.ToStyleClass(),
							Color = toRgbColor(shiftCategory?.DisplayColor)
						};
					}
				case SchedulePartView.FullDayAbsence:
					{
						var personAbsence = s.ScheduleDay.PersonAbsenceCollection()
							.OrderBy(a => a.Layer.Payload.Priority)
							.ThenByDescending(a => s.ScheduleDay.PersonAbsenceCollection().IndexOf(a))
							.First();
						return new FullDayAbsencePeriodViewModel
						{
							Title = personAbsence.Layer.Payload.Description.Name,
							Summary = toFormattedTimeSpan(s.Projection.ContractTime()),
							StyleClassName = personAbsence.Layer.Payload.DisplayColor.ToStyleClass(),
							Color = toRgbColor(personAbsence.Layer.Payload.DisplayColor)
						};
					}
				default:
					{
						return new PeriodViewModel
						{
							Title = Resources.NotScheduled
						};
					}
			}
		}

		private static string toFormattedTimeSpan(TimeSpan timespan)
		{
			return TimeHelper.GetLongHourMinuteTimeString(timespan, CultureInfo.CurrentUICulture);
		}

		private TimePeriod? getOpenHourPeriod(WeekScheduleDayDomainData weekScheduleDayDomainData)
		{
			var openHour = _siteOpenHourProvider.GetSiteOpenHourPeriod(weekScheduleDayDomainData.Date);
			if (!openHour.HasValue)
			{
				openHour = _scheduledSkillOpenHourProvider.GetSkillOpenHourPeriod(weekScheduleDayDomainData.ScheduleDay);
				if (openHour != null && openHour.Value.EndTime.Days > 0)
				{
					return new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1));
				}
			}
			return openHour;
		}

		private static string toRgbColor(Color? color)
		{
			return color == null
				? string.Empty
				: $"rgb({color.Value.R},{color.Value.G},{color.Value.B})";
		}
	}
}