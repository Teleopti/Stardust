using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeTimeLineHoursViewModelFactory : IShiftTradeTimeLineHoursViewModelFactory
	{
		private readonly TimeZoneInfo _timeZone;
		private readonly CultureInfo _culture;
		private readonly ICreateHourText _createHourText;

		public ShiftTradeTimeLineHoursViewModelFactory(TimeZoneInfo timeZone, CultureInfo culture, ICreateHourText createHourText)
		{
			_timeZone = timeZone;
			_culture = culture;
			_createHourText = createHourText;
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
						           HourText = _createHourText.CreateText(timeLinePeriod.EndDateTime, _timeZone, _culture),
						           LengthInMinutesToDisplay = timeLinePeriod.EndDateTime.Minute
					           };
				shiftEndRounded = timeLinePeriod.EndDateTime.AddMinutes(-timeLinePeriod.EndDateTime.Minute);
			}

			for (var time = shiftStartRounded; time < shiftEndRounded; time = time.AddHours(1))
			{
				hourList.Add(new ShiftTradeTimeLineHoursViewModel
					             {
										 HourText = _createHourText.CreateText(time, _timeZone, _culture),
						             LengthInMinutesToDisplay = 60
					             });
			}

			if (lastHour != null)
				hourList.Add(lastHour);

			return hourList;
		}

	}
}