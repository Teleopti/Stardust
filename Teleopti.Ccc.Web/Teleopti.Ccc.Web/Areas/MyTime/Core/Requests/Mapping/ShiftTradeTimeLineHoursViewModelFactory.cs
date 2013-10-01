using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeTimeLineHoursViewModelFactory : IShiftTradeTimeLineHoursViewModelFactory
	{
		private readonly ICreateHourText _createHourText;
		private readonly TimeZoneInfo _timeZone;

		public ShiftTradeTimeLineHoursViewModelFactory(ICreateHourText createHourText, IUserTimeZone timeZone)
		{
			_createHourText = createHourText;
			_timeZone = timeZone.TimeZone();
		}


		public IEnumerable<ShiftTradeTimeLineHoursViewModel> CreateTimeLineHours(DateTimePeriod timeLinePeriod)
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
						             LengthInMinutesToDisplay = lengthInMinutes,
									 StartTime = TimeZoneHelper.ConvertFromUtc(timeLinePeriod.StartDateTime, _timeZone),
									 EndTime = TimeZoneHelper.ConvertFromUtc(timeLinePeriod.StartDateTime.AddMinutes(lengthInMinutes), _timeZone)
					             });
				shiftStartRounded = timeLinePeriod.StartDateTime.AddMinutes(lengthInMinutes);
			}
			if (timeLinePeriod.EndDateTime.Minute != 0)
			{
				lastHour = new ShiftTradeTimeLineHoursViewModel
					           {
						           HourText = _createHourText.CreateText(timeLinePeriod.EndDateTime),
						           LengthInMinutesToDisplay = timeLinePeriod.EndDateTime.Minute,
								   StartTime = TimeZoneHelper.ConvertFromUtc(timeLinePeriod.EndDateTime.AddMinutes(-timeLinePeriod.EndDateTime.Minute), _timeZone),
								   EndTime = TimeZoneHelper.ConvertFromUtc(timeLinePeriod.EndDateTime, _timeZone)
					           };
				shiftEndRounded = timeLinePeriod.EndDateTime.AddMinutes(-timeLinePeriod.EndDateTime.Minute);
			}

			for (var time = shiftStartRounded; time < shiftEndRounded; time = time.AddHours(1))
			{
				hourList.Add(new ShiftTradeTimeLineHoursViewModel
					             {
									 HourText = _createHourText.CreateText(time),
						             LengthInMinutesToDisplay = 60,
									 StartTime = TimeZoneHelper.ConvertFromUtc(time, _timeZone),
									 EndTime = TimeZoneHelper.ConvertFromUtc(time.AddMinutes(60), _timeZone)
					             });
			}

			if (lastHour != null)
				hourList.Add(lastHour);

			return hourList;
		}

	}
}