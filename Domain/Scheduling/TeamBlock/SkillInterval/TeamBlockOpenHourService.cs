using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval
{
	public interface ITeamBlockOpenHourService
	{
		TimePeriod? OpenHoursForTeamBlock(ITeamBlockInfo teamBlockInfo, ISchedulingResultStateHolder schedulingResultStateHolder);
	}

	public class TeamBlockOpenHourService : ITeamBlockOpenHourService
	{
		private readonly IOpenHourForDate _openHourForDate;
		private readonly ICreateSkillIntervalDataPerDateAndActivity _createSkillIntervalDataPerDateAndActivity;

		public TeamBlockOpenHourService(IOpenHourForDate openHourForDate, ICreateSkillIntervalDataPerDateAndActivity createSkillIntervalDataPerDateAndActivity)
		{
			_openHourForDate = openHourForDate;
			_createSkillIntervalDataPerDateAndActivity = createSkillIntervalDataPerDateAndActivity;
		}

		public TimePeriod? OpenHoursForTeamBlock(ITeamBlockInfo teamBlockInfo, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>> dayIntervalDataPerDateAndActivity;
			dayIntervalDataPerDateAndActivity = _createSkillIntervalDataPerDateAndActivity.CreateFor(teamBlockInfo, schedulingResultStateHolder);
			var openHoursForBlock = new TimePeriod(TimeSpan.MaxValue, TimeSpan.MinValue);
			foreach (var dateOnly in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
			{
				var openHours = _openHourForDate.OpenHours(dateOnly, dayIntervalDataPerDateAndActivity[dateOnly]);
				if (!openHours.HasValue)
					return null;

				var narrowed = getNarrowTimePeriod(openHoursForBlock, openHours.Value);
				if (!narrowed.HasValue)
					return null;

				openHoursForBlock = narrowed.Value;
			}

			return openHoursForBlock;
		}

		private TimePeriod? getNarrowTimePeriod(TimePeriod existingTimePeriod, TimePeriod newTimePeriod)
		{
			if (!newTimePeriod.Intersect(existingTimePeriod))
				return null;

			var newStart = existingTimePeriod.StartTime;
			var newEnd = existingTimePeriod.EndTime;

			if (newTimePeriod.StartTime > newStart)
				newStart = newTimePeriod.StartTime;

			if (newTimePeriod.EndTime < newEnd)
				newEnd = newTimePeriod.EndTime;

			return new TimePeriod(newStart, newEnd);
		}
	}

	

	
}