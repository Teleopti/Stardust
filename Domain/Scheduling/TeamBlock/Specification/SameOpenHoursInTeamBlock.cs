using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification
{
	public interface ISameOpenHoursInTeamBlock
	{
		bool Check(IEnumerable<ISkillDay> allSkillDays, ITeamBlockInfo skillDays, IGroupPersonSkillAggregator groupPersonSkillAggregator);
	}

	public class SameOpenHoursInTeamBlock : ISameOpenHoursInTeamBlock
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

			var dates = skillIntervalDataPerDateAndActivity.Keys;
			TimePeriod? sampleOpenHour = null;
			foreach (var date in dates)
			{
				sampleOpenHour = _openHourForDate.OpenHours(date, skillIntervalDataPerDateAndActivity[date]);
				if(sampleOpenHour != null)
					break;
			}
			var blockPeriod = teamBlockInfo.BlockInfo.BlockPeriod;
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