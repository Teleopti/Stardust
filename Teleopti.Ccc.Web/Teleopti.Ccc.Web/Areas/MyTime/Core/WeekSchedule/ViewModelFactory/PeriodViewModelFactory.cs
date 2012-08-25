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
		public IEnumerable<PeriodViewModel> CreatePeriodViewModels(IVisualLayerCollection visualLayerCollection, TimePeriod minMaxTime)
		{
			foreach (var visualLayer in visualLayerCollection)
			{
				MeetingViewModel meetingModel = null;

				var meetingPayload = visualLayer.Payload as IMeetingPayload;
				if (meetingPayload != null)
				{
					meetingModel = new MeetingViewModel
					{
						Location = meetingPayload.Meeting.GetLocation(new NoFormatting()),
						Title = meetingPayload.Meeting.GetSubject(new NoFormatting())
					};
				}

				var dateTimePeriod = visualLayer.Period;
				var timePeriod = dateTimePeriod.TimePeriod(TeleoptiPrincipal.Current.Regional.TimeZone);

				var startTime = timePeriod.StartTime;
				var endTime = timePeriod.EndTime;

				// make sure the shifts over midnight are displayed on the same day
				if (startTime < minMaxTime.StartTime)
				{
					startTime+=TimeSpan.FromDays(1);
				}

				if (endTime < minMaxTime.StartTime)
				{
					endTime += TimeSpan.FromDays(1);
				}

				yield return
					new PeriodViewModel
					{
						Summary =
							TimeHelper.GetLongHourMinuteTimeString(dateTimePeriod.ElapsedTime(),
																   CultureInfo.CurrentUICulture),
						Title = visualLayer.DisplayDescription().Name,
						TimeSpan =
							dateTimePeriod.TimePeriod(TeleoptiPrincipal.Current.Regional.TimeZone).
							ToShortTimeString(CultureInfo.CurrentUICulture),
						StyleClassName = colorToString(visualLayer.DisplayColor()),
						Meeting = meetingModel,
						Color = visualLayer.DisplayColor().ToCSV(),
						StartPositionPercentage = (decimal)(startTime - minMaxTime.StartTime).Ticks / (minMaxTime.EndTime - minMaxTime.StartTime).Ticks,
						EndPositionPercentage = (decimal)(endTime - minMaxTime.StartTime).Ticks / (minMaxTime.EndTime - minMaxTime.StartTime).Ticks
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