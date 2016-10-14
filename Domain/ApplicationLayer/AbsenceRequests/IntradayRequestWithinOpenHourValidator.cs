using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class IntradayRequestWithinOpenHourValidator : IIntradayRequestWithinOpenHourValidator
	{
		public OpenHourStatus Validate(ISkill skill, DateTimePeriod requestPeriod)
		{
			var allOpenHours = new List<TimePeriod>();
			foreach (var wdt in skill.WorkloadCollection)
			{
				var forecastDayTemplate1 =
					(WorkloadDayTemplate) wdt.GetTemplate(TemplateTarget.Workload, requestPeriod.StartDateTime.DayOfWeek);
				if (forecastDayTemplate1.OpenHourList.Any())
				{
					allOpenHours.AddRange(forecastDayTemplate1.OpenHourList);
				}
			}
			var periodOf24 = new TimePeriod(new TimeSpan(0, 0, 0), new TimeSpan(1, 0, 0,0));
			if(allOpenHours.Contains(periodOf24)) return OpenHourStatus.WithinOpenHour;
			if (allOpenHours.Any())
			{
				var openHours = new TimePeriod(allOpenHours.Min(x=>x.StartTime),allOpenHours.Max(y=>y.EndTime));
				var startDateTime = requestPeriod.StartDateTime.Date.Add(openHours.StartTime);
				var localStartSpan = TimeZoneHelper.ConvertToUtc(startDateTime, skill.TimeZone).TimeOfDay;
				var endDateTime = requestPeriod.StartDateTime.Date.Add(openHours.EndTime);
				var localEndSpan = TimeZoneHelper.ConvertToUtc(endDateTime, skill.TimeZone).TimeOfDay;
				var localOpenHours = new TimePeriod(localStartSpan, localEndSpan);
				if (requestPeriod.TimePeriod(TimeZoneInfo.Utc).Intersect(localOpenHours))
					return OpenHourStatus.WithinOpenHour;
				return OpenHourStatus.OutsideOpenHour;
			}
			return OpenHourStatus.MissingOpenHour;
		}
	}
	public interface IIntradayRequestWithinOpenHourValidator
	{
		OpenHourStatus Validate(ISkill skill, DateTimePeriod requestPeriod);
	}

	public enum OpenHourStatus
	{
		WithinOpenHour,
		OutsideOpenHour,
		MissingOpenHour
	}
}