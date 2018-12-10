using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification
{
	public class SameOpenHoursInTeamBlock
	{
		private readonly IOpenHourForDate _openHourForDate;
		private readonly CreateSkillIntervalDataPerDateAndActivity _createSkillIntervalDataPerDateAndActivity;

		public SameOpenHoursInTeamBlock(IOpenHourForDate openHourForDate,
			CreateSkillIntervalDataPerDateAndActivity createSkillIntervalDataPerDateAndActivity)
		{
			_openHourForDate = openHourForDate;
			_createSkillIntervalDataPerDateAndActivity = createSkillIntervalDataPerDateAndActivity;
		}

		public bool Check(IEnumerable<ISkillDay> allSkillDays, ITeamBlockInfo teamBlockInfo, IGroupPersonSkillAggregator groupPersonSkillAggregator)
		{
			var skillIntervalDataPerDateAndActivity = _createSkillIntervalDataPerDateAndActivity.CreateFor(teamBlockInfo, allSkillDays, groupPersonSkillAggregator);
			var blockPeriod = teamBlockInfo.BlockInfo.BlockPeriod;
			var dates = skillIntervalDataPerDateAndActivity.Keys;
			TimePeriod? sampleOpenHour = null;
			foreach (var date in dates)
			{
				if(!blockPeriod.Contains(date))
					continue;
				sampleOpenHour = _openHourForDate.OpenHours(date, skillIntervalDataPerDateAndActivity[date]);
				if(sampleOpenHour != null)
					break;
			}
			
			foreach (var dateOnly in dates)
			{
				if (!blockPeriod.Contains(dateOnly))
					continue;

				var compareOpenHour = _openHourForDate.OpenHours(dateOnly, skillIntervalDataPerDateAndActivity[dateOnly]);
				if (compareOpenHour == null) continue;
				if (compareOpenHour != sampleOpenHour)
					return false;
			}

			return true;
		}
	}
}