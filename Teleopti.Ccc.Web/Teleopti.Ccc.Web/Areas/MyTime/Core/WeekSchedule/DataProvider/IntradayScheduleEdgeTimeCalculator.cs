using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.Web.Core.Extensions;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider
{
	public class IntradayScheduleEdgeTimeCalculator : IIntradayScheduleEdgeTimeCalculator
	{
		private readonly IWeekScheduleDomainDataProvider _weekScheduleDomainDataProvider;
		private readonly IUserTimeZone _timeZone;
		private readonly INow _now;


		public IntradayScheduleEdgeTimeCalculator(IWeekScheduleDomainDataProvider weekScheduleDomainDataProvider, IUserTimeZone timeZone, INow now)
		{
			_weekScheduleDomainDataProvider = weekScheduleDomainDataProvider;
			_timeZone = timeZone;
			_now = now;
		}

		public DateTime GetStartTime(DateOnly date)
		{
			var daySchedule = _weekScheduleDomainDataProvider.GetDaySchedule(date);

			if (!daySchedule.ScheduleDay.Projection.HasLayers)
			{
				if (new DateOnly(_now.UtcDateTime())== date)
				{
					return TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime().AddHours(1), _timeZone.TimeZone());
				}
				return new DateTime(date.Year, date.Month, date.Day, 8, 0, 0);
			}

			return TimeZoneHelper.ConvertFromUtc(daySchedule.ScheduleDay.Projection.First().Period.StartDateTime, _timeZone.TimeZone());
		}

		public DateTime GetEndTime(DateOnly date)
		{
			var daySchedule = _weekScheduleDomainDataProvider.GetDaySchedule(date);

			if (!daySchedule.ScheduleDay.Projection.HasLayers)
			{
				if (new DateOnly(_now.UtcDateTime()) == date)
				{
					return TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime().AddHours(1), _timeZone.TimeZone());
				}
				return new DateTime(date.Year, date.Month, date.Day, 17, 0, 0);
			}

			return TimeZoneHelper.ConvertFromUtc(daySchedule.ScheduleDay.Projection.Last().Period.EndDateTime, _timeZone.TimeZone());
		}

		public IntradayScheduleEdgeTime GetSchedulePeriodForCurrentUser(DateOnly date)
		{
			return new IntradayScheduleEdgeTime
			{
				StartDateTime = GetStartTime(date).ToString(DateTimeFormatExtensions.FixedDateTimeFormat),
				EndDateTime = GetEndTime(date).ToString(DateTimeFormatExtensions.FixedDateTimeFormat)
			};
		}
	}
}