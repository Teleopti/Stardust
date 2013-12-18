using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval
{
	public interface IOpenHourForDate
	{
		TimePeriod OpenHours(DateOnly dateOnly, IDictionary<IActivity, IList<ISkillIntervalData>> dayIntervalDataPerActivityForDate);
	}

	public class OpenHourForDate : IOpenHourForDate
	{
		private readonly ISkillIntervalDataOpenHour _skillIntervalDataOpenHour;

		public OpenHourForDate(ISkillIntervalDataOpenHour skillIntervalDataOpenHour)
		{
			_skillIntervalDataOpenHour = skillIntervalDataOpenHour;
		}

		public TimePeriod OpenHours(DateOnly dateOnly, IDictionary<IActivity, IList<ISkillIntervalData>> dayIntervalDataPerActivityForDate)
		{
			var minOpen = TimeSpan.MaxValue;
			var maxOpen = TimeSpan.MinValue;
			foreach (var activity in dayIntervalDataPerActivityForDate.Keys)
			{
				var openPeriod = _skillIntervalDataOpenHour.GetOpenHours(dayIntervalDataPerActivityForDate[activity], dateOnly);

				if (openPeriod.StartTime < minOpen)
					minOpen = openPeriod.StartTime;

				if (openPeriod.EndTime > maxOpen)
					maxOpen = openPeriod.EndTime;
			}

			return new TimePeriod(minOpen, maxOpen);
		}
	}
}