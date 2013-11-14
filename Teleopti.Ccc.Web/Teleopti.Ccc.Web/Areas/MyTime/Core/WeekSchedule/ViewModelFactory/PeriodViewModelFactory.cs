using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class PeriodViewModelFactory : IPeriodViewModelFactory
    {
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

                //yield return
                newList.Add(new PeriodViewModel
                                {
                                    Summary =
                                        TimeHelper.GetLongHourMinuteTimeString(visualLayer.Period.ElapsedTime(),
                                                                               CultureInfo.CurrentUICulture),
                                    Title = visualLayer.DisplayDescription().Name,
                                    TimeSpan =
                                        visualLayer.Period.TimePeriod(TeleoptiPrincipal.Current.Regional.TimeZone).
                                        ToShortTimeString(),
                                    StyleClassName = colorToString(visualLayer.DisplayColor()),
                                    Meeting = meetingModel,
                                    Color = visualLayer.DisplayColor().ToCSV(),
                                    StartPositionPercentage =
                                        (decimal)
                                        (visualLayer.VisualPeriod.TimePeriod(TeleoptiPrincipal.Current.Regional.TimeZone).
                                             StartTime - minMaxTime.StartTime).Ticks/
                                        (minMaxTime.EndTime - minMaxTime.StartTime).Ticks,
                                    EndPositionPercentage =
                                        (decimal)
                                        (visualLayer.VisualPeriod.TimePeriod(TeleoptiPrincipal.Current.Regional.TimeZone).
                                             EndTime - minMaxTime.StartTime).Ticks/
                                        (minMaxTime.EndTime - minMaxTime.StartTime).Ticks
                                });

            }
            return newList;
        }


		private static string colorToString(Color color)
		{
			var colorCode = color.ToArgb();
			return string.Concat("color_", ColorTranslator.ToHtml(Color.FromArgb(colorCode)).Replace("#", ""));
		}

	}
}