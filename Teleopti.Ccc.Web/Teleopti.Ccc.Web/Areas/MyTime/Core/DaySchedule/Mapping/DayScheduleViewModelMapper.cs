using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Requests;
using Teleopti.Ccc.Infrastructure.Staffing;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.Common;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.DaySchedule;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.WeekSchedule;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.Web.Core.Extensions;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.DaySchedule.Mapping
{
	public class DayScheduleViewModelMapper
	{
		private readonly IPeriodViewModelFactory _periodViewModelFactory;
		private readonly IHeaderViewModelFactory _headerViewModelFactory;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly INow _now;
		private readonly OvertimeAvailabilityViewModelMapper _overtimeMapper;
		private readonly IRequestsViewModelFactory _requestsViewModelFactory;
		private readonly ISiteOpenHourProvider _siteOpenHourProvider;
		private readonly IScheduledSkillOpenHourProvider _scheduledSkillOpenHourProvider;
		private readonly ILicenseAvailability _licenseAvailability;
		private readonly IToggleManager _toggleManager;
		private readonly IStaffingDataAvailablePeriodProvider _staffingDataAvailablePeriodProvider;

		public DayScheduleViewModelMapper(IPeriodViewModelFactory periodViewModelFactory,
			IHeaderViewModelFactory headerViewModelFactory,
			ILoggedOnUser loggedOnUser, INow now,
			OvertimeAvailabilityViewModelMapper overtimeMapper,
			IRequestsViewModelFactory requestsViewModelFactory,
			ISiteOpenHourProvider siteOpenHourProvider,
			IScheduledSkillOpenHourProvider scheduledSkillOpenHourProvider,
			ILicenseAvailability licenseAvailability,
			IToggleManager toggleManager,
			IStaffingDataAvailablePeriodProvider staffingDataAvailablePeriodProvider)
		{
			_periodViewModelFactory = periodViewModelFactory;
			_headerViewModelFactory = headerViewModelFactory;
			_loggedOnUser = loggedOnUser;
			_now = now;
			_overtimeMapper = overtimeMapper;
			_requestsViewModelFactory = requestsViewModelFactory;
			_siteOpenHourProvider = siteOpenHourProvider;
			_scheduledSkillOpenHourProvider = scheduledSkillOpenHourProvider;
			_licenseAvailability = licenseAvailability;
			_toggleManager = toggleManager;
			_staffingDataAvailablePeriodProvider = staffingDataAvailablePeriodProvider;
		}

		public DayScheduleViewModel Map(DayScheduleDomainData dayScheduleDomainData, bool loadOpenHourPeriod = false)
		{
			var currentUser = _loggedOnUser.CurrentUser();
			var timeZone = currentUser.PermissionInformation.DefaultTimeZone();

			var daylightSavingAdjustment = TimeZoneHelper.GetDaylightChanges(
				timeZone, _now.UtcDateTime().Year);
			var daylightModel = daylightSavingAdjustment != null
				? new DaylightSavingsTimeAdjustmentViewModel(daylightSavingAdjustment)
				{
					EnteringDST = dayScheduleDomainData.Date.Date.Equals(TimeZoneHelper.ConvertFromUtc(daylightSavingAdjustment.Start, timeZone).Date),
					LocalDSTStartTimeInMinutes = (int)TimeZoneHelper.ConvertFromUtc(daylightSavingAdjustment.Start, timeZone).TimeOfDay.TotalMinutes
				}
				: null;

			var viewModel = new DayScheduleViewModel
			{
				Date = dayScheduleDomainData.Date.ToFixedClientDateOnlyFormat(),
				BaseUtcOffsetInMinutes = timeZone.BaseUtcOffset.TotalMinutes,
				DaylightSavingTimeAdjustment = daylightModel,
				TimeLine = createTimeLine(dayScheduleDomainData.MinMaxTime, dayScheduleDomainData.Date, daylightSavingAdjustment, timeZone),
				RequestPermission = mapDaySchedulePermission(dayScheduleDomainData),
				ViewPossibilityPermission = dayScheduleDomainData.ViewPossibilityPermission,
				DatePickerFormat = DateTimeFormatExtensions.LocalizedDateFormat,
				Schedule = createDayViewModel(dayScheduleDomainData, loadOpenHourPeriod),
				AsmEnabled = dayScheduleDomainData.AsmEnabled,
				IsToday = dayScheduleDomainData.IsCurrentDay,
				CheckStaffingByIntraday = isCheckStaffingByIntradayForDay(currentUser.WorkflowControlSet, dayScheduleDomainData.Date),
				AbsenceProbabilityEnabled = currentUser.WorkflowControlSet?.AbsenceProbabilityEnabled ?? false,
				OvertimeProbabilityEnabled = isOvertimeProbabilityEnabled(dayScheduleDomainData.Date),
				UnReadMessageCount = dayScheduleDomainData.UnReadMessageCount,
				ShiftTradeRequestSetting = _requestsViewModelFactory.CreateShiftTradePeriodViewModel(),
				StaffingInfoAvailableDays = StaffingInfoAvailableDaysProvider.GetDays(_toggleManager) + 1
			};
			viewModel.Schedule.Periods = projections(dayScheduleDomainData);

			return viewModel;
		}

		private bool isOvertimeProbabilityEnabled(DateOnly date)
		{
			var currentUser = _loggedOnUser.CurrentUser();
			var overtimeProbabilityEnabled = currentUser.WorkflowControlSet?.OvertimeProbabilityEnabled != null
											 && currentUser.WorkflowControlSet.OvertimeProbabilityEnabled
											 && isOvertimeProbabilityLicenseAvailable();
			if (!overtimeProbabilityEnabled)
				return false;

			var isStaffingDataAvailable = _staffingDataAvailablePeriodProvider.GetPeriodsForOvertime(currentUser, date).Any();
			return isStaffingDataAvailable;
		}

		private bool isOvertimeProbabilityLicenseAvailable()
		{
			return _licenseAvailability.IsLicenseEnabled(DefinedLicenseOptionPaths.TeleoptiCccOvertimeAvailability)
				   || _licenseAvailability.IsLicenseEnabled(DefinedLicenseOptionPaths.TeleoptiWfmOvertimeRequests);
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

		private DayViewModel createDayViewModel(DayScheduleDomainData dayScheduleDomainData, bool loadOpenHourPeriod = false)
		{
			var personAssignment = dayScheduleDomainData.ScheduleDay?.PersonAssignment();
			var significantPartForDisplay = dayScheduleDomainData.ScheduleDay?.SignificantPartForDisplay();
			var dayViewModel = new DayViewModel
			{
				Date = dayScheduleDomainData.Date.ToShortDateString(),
				FixedDate = dayScheduleDomainData.Date.ToFixedClientDateOnlyFormat(),
				DayOfWeekNumber = (int)dayScheduleDomainData.Date.DayOfWeek,
				RequestsCount = dayScheduleDomainData.PersonRequestCount,
				ProbabilityClass = dayScheduleDomainData.ProbabilityClass,
				ProbabilityText = dayScheduleDomainData.ProbabilityText,
				State = dayScheduleDomainData.Date == new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone())) ? SpecialDateState.Today : 0,
				Header = _headerViewModelFactory.CreateModel(dayScheduleDomainData.ScheduleDay),
				Note = dayScheduleDomainData.ScheduleDay == null ? null : map(dayScheduleDomainData.ScheduleDay.PublicNoteCollection()),
				SeatBookings = dayScheduleDomainData.SeatBookingInformation,
				Summary = summary(dayScheduleDomainData),
				HasOvertime = personAssignment != null && personAssignment.ShiftLayers.OfType<OvertimeShiftLayer>().Any(),
				HasMainShift = personAssignment != null && personAssignment.ShiftLayers.OfType<MainShiftLayer>().Any(),
				IsFullDayAbsence = significantPartForDisplay == SchedulePartView.FullDayAbsence,
				IsDayOff = significantPartForDisplay == SchedulePartView.DayOff,
				OvertimeAvailabililty = overtimeAvailability(dayScheduleDomainData),
				Availability = dayScheduleDomainData.Availability,
				OpenHourPeriod = loadOpenHourPeriod ? getOpenHourPeriod(dayScheduleDomainData) : null
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

		private RequestPermission map(DayScheduleDomainData day)
		{
			return new RequestPermission
			{
				AbsenceReportPermission = day.AbsenceReportPermission,
				AbsenceRequestPermission = day.AbsenceRequestPermission,
				OvertimeRequestPermission = day.OvertimeRequestPermission,
				OvertimeAvailabilityPermission = day.OvertimeAvailabilityPermission,
				PersonAccountPermission = day.PersonAccountPermission,
				ShiftTradeBulletinBoardPermission = day.ShiftTradeBulletinBoardPermission,
				ShiftExchangePermission = day.ShiftExchangePermission,
				TextRequestPermission = day.TextRequestPermission
			};
		}

		private RequestPermission mapDaySchedulePermission(DayScheduleDomainData day)
		{
			var permission = map(day);
			permission.ShiftTradeRequestPermission = day.ShiftTradeRequestPermission;
			return permission;
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
			var localDayLightTimeStart =
				isOnEnteringDstDay ? TimeZoneHelper.ConvertFromUtc(daylightTime.Start, timezone) : DateTime.MinValue;
			var isCrossDSTStartTime = timelinePeriod.Contains(localDayLightTimeStart.TimeOfDay);

			var diff = endTime - startTime;

			if (isOnEnteringDstDay && isCrossDSTStartTime)
			{
				diff = getAjustedTimeSpanByDayLightTime(diff, daylightTime);
			}

			var timesCount = times.Count;
			for (var i = 0; i < timesCount; i++)
			{
				var time = times[i];
				if (isOnEnteringDstDay && isCrossDSTStartTime && isTimeSpanInDSTPeriod(time, localDayLightTimeStart))
				{
					time = getAjustedTimeSpanByDayLightTime(time, daylightTime);
				}

				yield return new TimeLineViewModel
				{
					Time = times[i],
					TimeLineDisplay = DateTime.MinValue.Add(times[i]).ToLocalizedTimeFormat(),
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

		private IEnumerable<PeriodViewModel> projections(DayScheduleDomainData day)
		{
			var projectionList = new List<IVisualLayer>();
			if (day.ProjectionYesterday != null)
			{
				projectionList.AddRange(day.ProjectionYesterday);
			}

			if (day.Projection != null)
			{
				projectionList.AddRange(day.Projection);
			}

			var periodViewModelFactory = _periodViewModelFactory;
			var minMaxTime = adjustMinEndTime(day.MinMaxTime);

			var periodsViewModels = periodViewModelFactory.CreatePeriodViewModelsForDay(projectionList, minMaxTime, day.Date,
				day.ScheduleDay?.TimeZone, day.ScheduleDay.Person, true);

			periodsViewModels = periodsViewModels ?? new PeriodViewModel[0];

			var overtimeAvailabilityPeriodViewModels =
				periodViewModelFactory.CreateOvertimeAvailabilityPeriodViewModels(day.OvertimeAvailability,
					day.OvertimeAvailabilityYesterday, minMaxTime) ?? new OvertimeAvailabilityPeriodViewModel[0];

			var yesterdayOvertimeAvailability = overtimeAvailabilityPeriodViewModels
				.Where(a => a.OvertimeAvailabilityYesterday != null).ToList().FirstOrDefault();

			if (yesterdayOvertimeAvailability != null && yesterdayOvertimeAvailability.StartTime.Date.Day != yesterdayOvertimeAvailability.EndTime.Date.Day)
			{
				var firstLayer = periodsViewModels.FirstOrDefault();
				if (firstLayer != null)
				{
					if (yesterdayOvertimeAvailability.EndTime >= firstLayer.StartTime)
					{
						yesterdayOvertimeAvailability.TimeSpan =
							TimeHelper.TimeOfDayFromTimeSpan(firstLayer.StartTime.TimeOfDay,
								CultureInfo.CurrentCulture) + " - " +
							TimeHelper.TimeOfDayFromTimeSpan(yesterdayOvertimeAvailability.EndTime.TimeOfDay, CultureInfo.CurrentCulture);
						var diff = (minMaxTime.EndTime - minMaxTime.StartTime).Ticks;
						yesterdayOvertimeAvailability.StartPositionPercentage = (decimal)(firstLayer.StartTime.TimeOfDay - minMaxTime.StartTime).Ticks / diff;
						yesterdayOvertimeAvailability.EndPositionPercentage = (decimal)(yesterdayOvertimeAvailability.EndTime - day.Date.Date.Add(minMaxTime.StartTime)).Ticks / diff;
					}
				}
			}

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

		private OvertimeAvailabilityViewModel overtimeAvailability(DayScheduleDomainData s)
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
					DefaultStartTime = TimeHelper.TimeOfDayFromTimeSpan(TimeSpan.FromHours(DefaultSchedulePeriodProvider.DefaultStartHour)),
					DefaultEndTime = TimeHelper.TimeOfDayFromTimeSpan(TimeSpan.FromHours(DefaultSchedulePeriodProvider.DefaultEndHour)),
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

		private static PeriodViewModel summary(DayScheduleDomainData s)
		{
			if (s.ScheduleDay == null || s.Projection == null)
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

		private TimePeriod? getOpenHourPeriod(DayScheduleDomainData weekScheduleDayDomainData)
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