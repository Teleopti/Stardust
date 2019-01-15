using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval
{
	public interface IOpenHourForDate
	{
		TimePeriod? OpenHours(DateOnly dateOnly, IDictionary<IActivity, IList<ISkillIntervalData>> dayIntervalDataPerActivityForDate);
	}

	public class OpenHourForDate : IOpenHourForDate
	{
		private readonly ISkillIntervalDataOpenHour _skillIntervalDataOpenHour;

		public OpenHourForDate(ISkillIntervalDataOpenHour skillIntervalDataOpenHour)
		{
			_skillIntervalDataOpenHour = skillIntervalDataOpenHour;
		}

		public TimePeriod? OpenHours(DateOnly dateOnly, IDictionary<IActivity, IList<ISkillIntervalData>> dayIntervalDataPerActivityForDate)
		{
			var minOpen = TimeSpan.MaxValue;
			var maxOpen = TimeSpan.MinValue;
			foreach (var activity in dayIntervalDataPerActivityForDate)
			{
				var openPeriod = _skillIntervalDataOpenHour.GetOpenHours(activity.Value, dateOnly);
				if(!openPeriod.HasValue)
					continue;

				if (openPeriod.Value.StartTime < minOpen)
					minOpen = openPeriod.Value.StartTime;

				if (openPeriod.Value.EndTime > maxOpen)
					maxOpen = openPeriod.Value.EndTime;
			}

			if (minOpen > maxOpen)
				return null;

			return new TimePeriod(minOpen, maxOpen);
		}
	}
}