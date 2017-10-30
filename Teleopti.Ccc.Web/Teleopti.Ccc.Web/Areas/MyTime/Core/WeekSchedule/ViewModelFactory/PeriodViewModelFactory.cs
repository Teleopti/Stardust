﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class PeriodViewModelFactory : IPeriodViewModelFactory
	{
		private readonly IUserTimeZone _timeZone;
		private readonly OvertimeAvailabilityViewModelMapper _overtimeMapper;

		public PeriodViewModelFactory(IUserTimeZone timeZone, OvertimeAvailabilityViewModelMapper overtimeMapper)
		{
			_timeZone = timeZone;
			_overtimeMapper = overtimeMapper;
		}

		public IEnumerable<PeriodViewModel> CreatePeriodViewModels(IEnumerable<IVisualLayer> visualLayerCollection, TimePeriod minMaxTime, DateOnly localDate, TimeZoneInfo timeZone)
		{
			var calendarDayExtractor = new VisualLayerCalendarDayExtractor();
			var layerExtendedList = calendarDayExtractor.CreateVisualPeriods(localDate, visualLayerCollection, timeZone);

			var newList = new List<PeriodViewModel>();

			foreach (var visualLayer in layerExtendedList)
			{
				MeetingViewModel meetingModel = null;

				var meetingPayload = visualLayer.Payload as IMeetingPayload;
				if (meetingPayload != null)
				{
					var formatter = new NoFormatting();
					meetingModel = new MeetingViewModel
					{
						Location = meetingPayload.Meeting.GetLocation(formatter),
						Title = meetingPayload.Meeting.GetSubject(formatter),
						Description = meetingPayload.Meeting.GetDescription(formatter)
					};
				}

				var isOvertimeLayer = visualLayer.DefinitionSet?.MultiplicatorType == MultiplicatorType.Overtime;

				var timezone = _timeZone.TimeZone();
				var timePeriod = visualLayer.VisualPeriod.TimePeriod(timezone);
				var startPositionPercentage =
					(decimal)(timePeriod.StartTime - minMaxTime.StartTime).Ticks /
					(minMaxTime.EndTime - minMaxTime.StartTime).Ticks;
				var endPositionPercentage =
					(decimal)(timePeriod.EndTime - minMaxTime.StartTime).Ticks /
					(minMaxTime.EndTime - minMaxTime.StartTime).Ticks;
				newList.Add(new PeriodViewModel
				{
					Summary =
						TimeHelper.GetLongHourMinuteTimeString(visualLayer.Period.ElapsedTime(),
							CultureInfo.CurrentUICulture),
					Title = visualLayer.DisplayDescription().Name,
					TimeSpan = visualLayer.Period.TimePeriod(timezone).ToShortTimeString(),
					StartTime = visualLayer.Period.StartDateTimeLocal(timezone),
					EndTime = visualLayer.Period.EndDateTimeLocal(timezone),
					StyleClassName = colorToString(visualLayer.DisplayColor()),
					Meeting = meetingModel,
					Color = visualLayer.DisplayColor().ToCSV(),
					StartPositionPercentage = startPositionPercentage,
					EndPositionPercentage = endPositionPercentage,
					IsOvertime = isOvertimeLayer
				});
			}
			return newList;
		}

		public IEnumerable<OvertimeAvailabilityPeriodViewModel> CreateOvertimeAvailabilityPeriodViewModels(IOvertimeAvailability overtimeAvailability, IOvertimeAvailability overtimeAvailabilityYesterday, TimePeriod minMaxTime)
		{
			var overtimeAvailabilityPeriods = new List<OvertimeAvailabilityPeriodViewModel>();
			if (overtimeAvailability != null)
			{
				var start = overtimeAvailability.StartTime.Value;
				var end = overtimeAvailability.EndTime.Value;
				var endPositionPercentage = (decimal)(end - minMaxTime.StartTime).Ticks / (minMaxTime.EndTime - minMaxTime.StartTime).Ticks;
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
					var endPositionPercentageYesterday = (decimal)(endYesterday.Subtract(new TimeSpan(1, 0, 0, 0)) - minMaxTime.StartTime).Ticks / (minMaxTime.EndTime - minMaxTime.StartTime).Ticks;
					overtimeAvailabilityPeriods.Add(new OvertimeAvailabilityPeriodViewModel
					{
						Title = Resources.OvertimeAvailabilityWeb,
						TimeSpan = TimeHelper.TimeOfDayFromTimeSpan(startYesterday, CultureInfo.CurrentCulture) + " - " + TimeHelper.TimeOfDayFromTimeSpan(endYesterday, CultureInfo.CurrentCulture),
						StartPositionPercentage = 0,
						EndPositionPercentage = endPositionPercentageYesterday,
						IsOvertimeAvailability = true,
						OvertimeAvailabilityYesterday = _overtimeMapper.Map(overtimeAvailabilityYesterday),
						Color = Color.Gray.ToCSV()
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