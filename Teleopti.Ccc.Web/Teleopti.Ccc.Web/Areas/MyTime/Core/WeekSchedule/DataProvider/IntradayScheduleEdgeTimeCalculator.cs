using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.Web.Core.Extensions;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider
{
	public class IntradayScheduleEdgeTimeCalculator : IIntradayScheduleEdgeTimeCalculator
	{
		private readonly IDayScheduleDomainDataProvider _dayScheduleDomainDataProvider;
		private readonly IUserTimeZone _timeZone;
		private readonly INow _now;

		public IntradayScheduleEdgeTimeCalculator(IDayScheduleDomainDataProvider dayScheduleDomainDataProvider, IUserTimeZone timeZone, INow now)
		{
			_dayScheduleDomainDataProvider = dayScheduleDomainDataProvider;
			_timeZone = timeZone;
			_now = now;
		}

		public DateTime GetStartTime(DateOnly date)
		{
			var daySchedule = _dayScheduleDomainDataProvider.GetDaySchedule(date);

			if (!daySchedule.Projection.HasLayers)
			{
				return TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime().AddHours(1), _timeZone.TimeZone());
			}

			return TimeZoneHelper.ConvertFromUtc(daySchedule.Projection.First().Period.StartDateTime, _timeZone.TimeZone());
		}

		public DateTime GetEndTime(DateOnly date)
		{
			var daySchedule = _dayScheduleDomainDataProvider.GetDaySchedule(date);

			if (!daySchedule.Projection.HasLayers)
			{
				return TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime().AddHours(1), _timeZone.TimeZone());
			}

			return TimeZoneHelper.ConvertFromUtc(daySchedule.Projection.Last().Period.EndDateTime, _timeZone.TimeZone());
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