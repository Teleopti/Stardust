using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using AutoMapper;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class PeriodViewModelFactory : IPeriodViewModelFactory
    {
		private readonly IMappingEngine _mapper;
		private readonly IUserTimeZone _timeZone;

		public PeriodViewModelFactory(IMappingEngine mapper, IUserTimeZone timeZone)
		{
			_mapper = mapper;
			_timeZone = timeZone;
		}

		public IEnumerable<PeriodViewModel> CreatePeriodViewModels(IEnumerable<IVisualLayer> visualLayerCollection, TimePeriod minMaxTime, DateTime localDate, TimeZoneInfo timeZone)
        {
            var calendarDayExtractor = new VisualLayerCalendarDayExtractor();
            var layerExtendedList = calendarDayExtractor.CreateVisualPeriods(localDate, visualLayerCollection, timeZone);

            var newList = new List<PeriodViewModel>();
            
            foreach (var visualLayer in layerExtendedList)
            {
                MeetingViewModel meetingModel = null;
                
                var meetingPayload = visualLayer.Payload as IMeetingPayload;
                if (meetingPayload!=null)
                {
                    meetingModel = new MeetingViewModel
                                        {
                                            Location = meetingPayload.Meeting.GetLocation(new NoFormatting()),
                                            Title = meetingPayload.Meeting.GetSubject(new NoFormatting()),
														  Description = meetingPayload.Meeting.GetDescription(new NoFormatting())
                                        };
                }

				var isOvertimeLayer = visualLayer.DefinitionSet != null && visualLayer.DefinitionSet.MultiplicatorType == MultiplicatorType.Overtime;

                newList.Add(new PeriodViewModel
                                {
                                    Summary =
                                        TimeHelper.GetLongHourMinuteTimeString(visualLayer.Period.ElapsedTime(),
                                                                               CultureInfo.CurrentUICulture),
                                    Title = visualLayer.DisplayDescription().Name,
                                    TimeSpan =
										visualLayer.Period.TimePeriod(_timeZone.TimeZone()).
                                        ToShortTimeString(),
                                    StyleClassName = colorToString(visualLayer.DisplayColor()),
                                    Meeting = meetingModel,
                                    Color = visualLayer.DisplayColor().ToCSV(),
                                    StartPositionPercentage =
                                        (decimal)
										(visualLayer.VisualPeriod.TimePeriod(_timeZone.TimeZone()).
                                             StartTime - minMaxTime.StartTime).Ticks/
                                        (minMaxTime.EndTime - minMaxTime.StartTime).Ticks,
                                    EndPositionPercentage =
                                        (decimal)
										(visualLayer.VisualPeriod.TimePeriod(_timeZone.TimeZone()).
                                             EndTime - minMaxTime.StartTime).Ticks/
                                        (minMaxTime.EndTime - minMaxTime.StartTime).Ticks,
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
						OvertimeAvailabilityYesterday = _mapper.Map<IOvertimeAvailability, OvertimeAvailabilityViewModel>(overtimeAvailabilityYesterday),
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