using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeTimeLineHoursViewModelFactory : IShiftTradeTimeLineHoursViewModelFactory
	{
		private readonly ICreateHourText _createHourText;
		private readonly IUserTimeZone _userTimeZone;

		public ShiftTradeTimeLineHoursViewModelFactory(ICreateHourText createHourText, IUserTimeZone userTimeZone)
		{
			_createHourText = createHourText;
			_userTimeZone = userTimeZone;
		}


		public IEnumerable<ShiftTradeTimeLineHoursViewModel> CreateTimeLineHours(DateTimePeriod timeLinePeriod)
		{
			var hourList = new List<ShiftTradeTimeLineHoursViewModel>();
			ShiftTradeTimeLineHoursViewModel lastHour = null;
			var shiftStartRounded = timeLinePeriod.StartDateTime;
			var shiftEndRounded = timeLinePeriod.EndDateTime;
			var timeZone = _userTimeZone.TimeZone();

			if (timeLinePeriod.StartDateTime.Minute != 0)
			{
				var lengthInMinutes = 60 - timeLinePeriod.StartDateTime.Minute;
				hourList.Add(new ShiftTradeTimeLineHoursViewModel
					             {
						             HourText = string.Empty,
						             LengthInMinutesToDisplay = lengthInMinutes,
									 StartTime = TimeZoneHelper.ConvertFromUtc(timeLinePeriod.StartDateTime, timeZone),
									 EndTime = TimeZoneHelper.ConvertFromUtc(timeLinePeriod.StartDateTime.AddMinutes(lengthInMinutes), timeZone)
					             });
				shiftStartRounded = timeLinePeriod.StartDateTime.AddMinutes(lengthInMinutes);
			}
			if (timeLinePeriod.EndDateTime.Minute != 0)
			{
				shiftEndRounded = timeLinePeriod.EndDateTime.AddMinutes(-timeLinePeriod.EndDateTime.Minute);
				lastHour = new ShiftTradeTimeLineHoursViewModel
					           {
								   HourText = _createHourText.CreateText(shiftEndRounded),
						           LengthInMinutesToDisplay = timeLinePeriod.EndDateTime.Minute,
								   StartTime = TimeZoneHelper.ConvertFromUtc(timeLinePeriod.EndDateTime.AddMinutes(-timeLinePeriod.EndDateTime.Minute), timeZone),
								   EndTime = TimeZoneHelper.ConvertFromUtc(timeLinePeriod.EndDateTime, timeZone)
					           };
			}

			for (var time = shiftStartRounded; time < shiftEndRounded; time = time.AddHours(1))
			{
				hourList.Add(new ShiftTradeTimeLineHoursViewModel
					             {
									 HourText = _createHourText.CreateText(time),
						             LengthInMinutesToDisplay = 60,
									 StartTime = TimeZoneHelper.ConvertFromUtc(time, timeZone),
									 EndTime = TimeZoneHelper.ConvertFromUtc(time.AddMinutes(60), timeZone)
					             });
			}

			if (lastHour != null)
				hourList.Add(lastHour);

			return hourList;
		}

	}
}