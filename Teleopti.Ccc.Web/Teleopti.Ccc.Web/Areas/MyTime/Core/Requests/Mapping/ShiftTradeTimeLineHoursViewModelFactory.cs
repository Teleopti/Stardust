using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeTimeLineHoursViewModelFactory
	{
		private readonly TimeZoneInfo _timeZone;
		private readonly CultureInfo _culture;

		public ShiftTradeTimeLineHoursViewModelFactory(TimeZoneInfo timeZone, CultureInfo culture)
		{
			_timeZone = timeZone;
			_culture = culture;
		}

		public  IEnumerable<ShiftTradeTimeLineHoursViewModel> CreateTimeLineHours(DateTimePeriod timeLinePeriod)
		{
			var hourList = new List<ShiftTradeTimeLineHoursViewModel>();
			ShiftTradeTimeLineHoursViewModel lastHour = null;
			var shiftStartRounded = timeLinePeriod.StartDateTime;
			var shiftEndRounded = timeLinePeriod.EndDateTime;

			if (timeLinePeriod.StartDateTime.Minute != 0)
			{
				var lengthInMinutes = 60 - timeLinePeriod.StartDateTime.Minute;
				hourList.Add(new ShiftTradeTimeLineHoursViewModel
					             {
						             HourText = string.Empty,
						             LengthInMinutesToDisplay = lengthInMinutes
					             });
				shiftStartRounded = timeLinePeriod.StartDateTime.AddMinutes(lengthInMinutes);
			}
			if (timeLinePeriod.EndDateTime.Minute != 0)
			{
				lastHour = new ShiftTradeTimeLineHoursViewModel
					           {
						           HourText = createHourText(timeLinePeriod.EndDateTime, _timeZone, _culture),
						           LengthInMinutesToDisplay = timeLinePeriod.EndDateTime.Minute
					           };
				shiftEndRounded = timeLinePeriod.EndDateTime.AddMinutes(-timeLinePeriod.EndDateTime.Minute);
			}

			for (var time = shiftStartRounded; time < shiftEndRounded; time = time.AddHours(1))
			{
				hourList.Add(new ShiftTradeTimeLineHoursViewModel
					             {
						             HourText = createHourText(time, _timeZone, _culture),
						             LengthInMinutesToDisplay = 60
					             });
			}

			if (lastHour != null)
				hourList.Add(lastHour);

			return hourList;
		}

		private static string createHourText(DateTime time, TimeZoneInfo timeZone, CultureInfo culture)
		{
			//rk - make a seperate service/interface for this?
			var localTime = TimeZoneHelper.ConvertFromUtc(time, timeZone);
			var hourString = string.Format(culture, localTime.ToShortTimeString());

			const string regex = "(\\:.*\\ )";
			var output = Regex.Replace(hourString, regex, " ");
			if (output.Contains(":"))
				output = localTime.Hour.ToString();

			return output;
		}
	}
}