﻿using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
    public class PeriodViewModelFactory : IPeriodViewModelFactory
    {
        public IEnumerable<PeriodViewModel> CreatePeriodViewModels(IVisualLayerCollection visualLayerCollection)
        {
            foreach (var visualLayer in visualLayerCollection)
            {
                MeetingViewModel meetingModel = null;
                
                var meetingPayload = visualLayer.Payload as IMeetingPayload;
                if (meetingPayload!=null)
                {
                    meetingModel = new MeetingViewModel
                                       {
                                           Location = meetingPayload.Meeting.Location,
                                           Title = meetingPayload.Meeting.Subject
                                       };
                }
                
                yield return
                    new PeriodViewModel
                        {
                            Summary =
                                TimeHelper.GetLongHourMinuteTimeString(visualLayer.Period.ElapsedTime(),
                                                                       CultureInfo.CurrentUICulture),
                            Title = visualLayer.DisplayDescription().Name,
                            TimeSpan =
                                visualLayer.Period.TimePeriod(TeleoptiPrincipal.Current.Regional.TimeZone).
                                ToShortTimeString(CultureInfo.CurrentUICulture),
                            StyleClassName = colorToString(visualLayer.DisplayColor()),
                            Meeting = meetingModel
                        };
            }
        }

        private static string colorToString(Color color)
        {
            var colorCode = color.ToArgb();
            return string.Concat("color_", ColorTranslator.ToHtml(Color.FromArgb(colorCode)).Replace("#", ""));
        }
    }
}