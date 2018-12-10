using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
					(WorkloadDayTemplate)wdt.GetTemplate(TemplateTarget.Workload, requestPeriod.StartDateTime.DayOfWeek);
				if (forecastDayTemplate1.OpenHourList.Any())
				{
					allOpenHours.AddRange(forecastDayTemplate1.OpenHourList);
				}
			}
			var periodOf24 = new TimePeriod(new TimeSpan(0, 0, 0), new TimeSpan(1, 0, 0, 0));
			if (allOpenHours.Contains(periodOf24)) return OpenHourStatus.WithinOpenHour;
			if (allOpenHours.Any())
			{
				foreach (var openHours in allOpenHours)
				{
					if (requestPeriod.TimePeriod(skill.TimeZone).Intersect(openHours))
						return OpenHourStatus.WithinOpenHour;
				}
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