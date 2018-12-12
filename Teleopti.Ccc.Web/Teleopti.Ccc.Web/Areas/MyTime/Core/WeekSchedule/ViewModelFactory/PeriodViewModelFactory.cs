using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.Common;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class PeriodViewModelFactory : IPeriodViewModelFactory
	{
		private readonly IUserTimeZone _timeZone;
		private readonly OvertimeAvailabilityViewModelMapper _overtimeMapper;
		private readonly INow _now;

		public PeriodViewModelFactory(IUserTimeZone timeZone, OvertimeAvailabilityViewModelMapper overtimeMapper, INow now)
		{
			_timeZone = timeZone;
			_overtimeMapper = overtimeMapper;
			_now = now;
		}

		public IEnumerable<PeriodViewModel> CreatePeriodViewModelsForWeek(IEnumerable<IVisualLayer> visualLayerCollection, TimePeriod minMaxTime, DateOnly localDate, TimeZoneInfo timeZone, IPerson person)
		{
			var calendarDayExtractor = new VisualLayerCalendarDayExtractor();
			var layerExtendedList = calendarDayExtractor.CreateVisualPeriods(localDate, visualLayerCollection, timeZone);
			
			var timezone = _timeZone.TimeZone();
			var totalLengthTicks = (minMaxTime.EndTime - minMaxTime.StartTime).Ticks;

			return layerExtendedList.Select(visualLayer => {
				var meetingModel = createMeetingViewModel(visualLayer);

				var isOvertimeLayer = visualLayer.DefinitionSet?.MultiplicatorType == MultiplicatorType.Overtime;

				var positionPercentage =
					getPeriodPositionPercentage(minMaxTime, visualLayer.VisualPeriod.TimePeriod(timezone),
						totalLengthTicks);

				return new PeriodViewModel
				{
					Summary =
						TimeHelper.GetLongHourMinuteTimeString(visualLayer.Period.ElapsedTime(),
							CultureInfo.CurrentUICulture),
					Title = visualLayer.Payload.ConfidentialDescription(person).Name,
					TimeSpan = visualLayer.Period.TimePeriod(timezone).ToShortTimeString(),
					StartTime = visualLayer.Period.StartDateTimeLocal(timezone),
					EndTime = visualLayer.Period.EndDateTimeLocal(timezone),
					StyleClassName = colorToString(visualLayer.Payload.ConfidentialDisplayColor(person)),
					Meeting = meetingModel,
					Color = visualLayer.Payload.ConfidentialDisplayColor(person).ToCSV(),
					StartPositionPercentage = positionPercentage.Start,
					EndPositionPercentage = positionPercentage.End,
					IsOvertime = isOvertimeLayer
				};
			}).ToArray();
		}

		public IEnumerable<PeriodViewModel> CreatePeriodViewModelsForDay(IEnumerable<IVisualLayer> visualLayerCollection, TimePeriod minMaxTime, DateOnly localDate, TimeZoneInfo timeZone, IPerson person, bool allowCrossNight)
		{
			var calendarDayExtractor = new VisualLayerCalendarDayExtractor();
			var layerExtendedList = calendarDayExtractor.CreateVisualPeriods(localDate, visualLayerCollection, timeZone, allowCrossNight);
			
			var timezone = _timeZone.TimeZone();
			var daylightTime = TimeZoneHelper.GetDaylightChanges(
				timezone, TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), timezone).Year);
			var isOnDSTStartDay = isEnteringDST(localDate, daylightTime, timezone);
			var localDaylightStartTime = getLocalDaylightStartTime(daylightTime, timezone);

			var totalLengthTicks = (minMaxTime.EndTime - minMaxTime.StartTime).Ticks;

			return layerExtendedList.Select(visualLayer => 
			{
				var meetingModel = createMeetingViewModel(visualLayer);
				var isOvertimeLayer = visualLayer.DefinitionSet?.MultiplicatorType == MultiplicatorType.Overtime;

				var localTimePeriod = getVisualLayerTimePeriod(visualLayer, localDate, timezone);

				PeriodPositionPercentage positionPercentage;
				if (isOnDSTStartDay && minMaxTime.Contains(localDaylightStartTime))
				{
					totalLengthTicks = totalLengthTicks - daylightTime.Delta.Ticks;
					positionPercentage = getPeriodPositionPercentage(minMaxTime, localTimePeriod,
						true, localDaylightStartTime, daylightTime, totalLengthTicks);
				}
				else
				{
					positionPercentage = getPeriodPositionPercentage(minMaxTime, localTimePeriod, totalLengthTicks);
				}

				return new PeriodViewModel
				{
					Summary =
						TimeHelper.GetLongHourMinuteTimeString(visualLayer.Period.ElapsedTime(),
							CultureInfo.CurrentUICulture),
					Title = visualLayer.Payload.ConfidentialDescription(person).Name,
					TimeSpan = visualLayer.Period.TimePeriod(timezone).ToShortTimeString(),
					StartTime = visualLayer.Period.StartDateTimeLocal(timezone),
					EndTime = visualLayer.Period.EndDateTimeLocal(timezone),
					StyleClassName = colorToString(visualLayer.Payload.ConfidentialDisplayColor(person)),
					Meeting = meetingModel,
					Color = visualLayer.Payload.ConfidentialDisplayColor(person).ToCSV(),
					StartPositionPercentage = positionPercentage.Start,
					EndPositionPercentage = positionPercentage.End,
					IsOvertime = isOvertimeLayer
				};
			}).OrderBy(l => l.StartTime);
		}

		private static TimePeriod getVisualLayerTimePeriod(VisualLayerForWebDisplay visualLayer, DateOnly localDate, TimeZoneInfo timezone)
		{
			var timePeriodStart = TimeZoneHelper.ConvertFromUtc(visualLayer.VisualPeriod.StartDateTime, timezone);
			var timePeriodEnd = TimeZoneHelper.ConvertFromUtc(visualLayer.VisualPeriod.EndDateTime, timezone);

			var timeSpanStart = timePeriodStart.TimeOfDay;
			var timeSpanEnd = timePeriodEnd.TimeOfDay;

			if (timePeriodStart.Date > localDate.Date)
			{
				timeSpanStart = timeSpanStart.Add(TimeSpan.FromDays(1));
			}

			if (timePeriodEnd.Date > localDate.Date)
			{
				timeSpanEnd = timeSpanEnd.Add(TimeSpan.FromDays(1));
			}

			return new TimePeriod(timeSpanStart, timeSpanEnd);
		}

		private static MeetingViewModel createMeetingViewModel(VisualLayerForWebDisplay visualLayer)
		{
			MeetingViewModel meetingModel = null;

			if (visualLayer.Payload is IMeetingPayload meetingPayload)
			{
				var formatter = new NoFormatting();
				meetingModel = new MeetingViewModel
				{
					Location = meetingPayload.Meeting.GetLocation(formatter),
					Title = meetingPayload.Meeting.GetSubject(formatter),
					Description = meetingPayload.Meeting.GetDescription(formatter)
				};
			}

			return meetingModel;
		}


		private PeriodPositionPercentage getPeriodPositionPercentage(TimePeriod minMaxTime, TimePeriod timePeriod,
			bool isOnDSTStartDay, TimeSpan localDaylightStartTime, DaylightTime daylightTime,
			long totalLengthTicks)
		{
			timePeriod = getAjustedTimePeriodByDaylightTime(timePeriod, isOnDSTStartDay, localDaylightStartTime, daylightTime);

			return getPeriodPositionPercentage(minMaxTime, timePeriod, totalLengthTicks);
		}

		private static PeriodPositionPercentage getPeriodPositionPercentage(TimePeriod minMaxTime, TimePeriod timePeriod,
			long totalLengthTicks)
		{
			var startPositionPercentage =
				(decimal) (timePeriod.StartTime - minMaxTime.StartTime).Ticks / totalLengthTicks;

			var endPositionPercentage = (decimal) (timePeriod.EndTime - minMaxTime.StartTime).Ticks / totalLengthTicks;

			return new PeriodPositionPercentage() {Start = startPositionPercentage, End = endPositionPercentage};
		}

		private struct PeriodPositionPercentage
		{
			public Decimal Start { get; set; }
			public Decimal End { get; set; }
		}

		private static TimeSpan getLocalDaylightStartTime(DaylightTime daylightTime, TimeZoneInfo localTimeZone)
		{
			if (daylightTime == null)
				return TimeSpan.Zero;
			return TimeZoneHelper.ConvertFromUtc(daylightTime.Start, localTimeZone).TimeOfDay;
		}

		private bool isEnteringDST(DateOnly localDate, DaylightTime daylightTime, TimeZoneInfo timeZone)
		{
			if (daylightTime == null)
				return false;

			return TimeZoneHelper.ConvertFromUtc(daylightTime.Start, timeZone).Date
					   .CompareTo(localDate.Date) == 0;
		}

		private TimePeriod getAjustedTimePeriodByDaylightTime(TimePeriod timePeriod, bool isOnDSTStartDay, TimeSpan localDaylightStartTime, DaylightTime daylightTime)
		{
			if (!isOnDSTStartDay) return timePeriod;

			if (timePeriod.StartTime >= localDaylightStartTime)
			{
				timePeriod = new TimePeriod(timePeriod.StartTime.Add(-daylightTime.Delta), timePeriod.EndTime);
			}

			if (timePeriod.EndTime >= localDaylightStartTime)
			{
				timePeriod = new TimePeriod(timePeriod.StartTime, timePeriod.EndTime.Add(-daylightTime.Delta));
			}

			return timePeriod;
		}

		public IEnumerable<OvertimeAvailabilityPeriodViewModel> CreateOvertimeAvailabilityPeriodViewModels(IOvertimeAvailability overtimeAvailability, IOvertimeAvailability overtimeAvailabilityYesterday, TimePeriod minMaxTime)
		{
			var overtimeAvailabilityPeriods = new List<OvertimeAvailabilityPeriodViewModel>(2);
			var diff = (minMaxTime.EndTime - minMaxTime.StartTime).Ticks;
			if (overtimeAvailability != null)
			{
				var start = overtimeAvailability.StartTime.Value;
				var end = overtimeAvailability.EndTime.Value;
				var endPositionPercentage = (decimal)(end - minMaxTime.StartTime).Ticks / diff;
				overtimeAvailabilityPeriods.Add(new OvertimeAvailabilityPeriodViewModel
				{
					Title = Resources.OvertimeAvailabilityWeb,
					TimeSpan = TimeHelper.TimeOfDayFromTimeSpan(start, CultureInfo.CurrentCulture) + " - " + TimeHelper.TimeOfDayFromTimeSpan(end, CultureInfo.CurrentCulture),
					StartPositionPercentage = (decimal)(start - minMaxTime.StartTime).Ticks / (minMaxTime.EndTime - minMaxTime.StartTime).Ticks,
					EndPositionPercentage = endPositionPercentage > 1 ? 1 : endPositionPercentage,
					IsOvertimeAvailability = true,
					Color = Color.Gray.ToCSV()
				});
			}

			if (overtimeAvailabilityYesterday != null)
			{
				var startYesterday = overtimeAvailabilityYesterday.StartTime.Value;
				var endYesterday = overtimeAvailabilityYesterday.EndTime.Value;
				if (endYesterday.Days >= 1)
				{
					var endPositionPercentageYesterday = (decimal)(endYesterday.Subtract(new TimeSpan(1, 0, 0, 0)) - minMaxTime.StartTime).Ticks / diff;

					overtimeAvailabilityPeriods.Add(new OvertimeAvailabilityPeriodViewModel
					{
						Title = Resources.OvertimeAvailabilityWeb,
						TimeSpan = TimeHelper.TimeOfDayFromTimeSpan(startYesterday, CultureInfo.CurrentCulture) + " - " + TimeHelper.TimeOfDayFromTimeSpan(endYesterday, CultureInfo.CurrentCulture),
						StartPositionPercentage = 0,
						EndPositionPercentage = endPositionPercentageYesterday,
						IsOvertimeAvailability = true,
						OvertimeAvailabilityYesterday = _overtimeMapper.Map(overtimeAvailabilityYesterday),
						Color = Color.Gray.ToCSV(),
						StartTime = overtimeAvailabilityYesterday.DateOfOvertime.Date.Add(overtimeAvailabilityYesterday.StartTime.Value),
						EndTime = overtimeAvailabilityYesterday.DateOfOvertime.Date.Add(overtimeAvailabilityYesterday.EndTime.Value)
					});
				}
			}
			return overtimeAvailabilityPeriods;
		}

		private static string colorToString(Color color)
		{
			var colorCode = color.ToArgb();
			return string.Concat("color_", ColorTranslator.ToHtml(Color.FromArgb(colorCode)).Replace("#", ""));
		}
	}
}