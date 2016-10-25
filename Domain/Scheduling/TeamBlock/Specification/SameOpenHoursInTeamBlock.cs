using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification
{
	public interface ISameOpenHoursInTeamBlock
	{
		bool Check(IEnumerable<ISkillDay> allSkillDays, ITeamBlockInfo skillDays);
	}

	public class SameOpenHoursInTeamBlock : ISameOpenHoursInTeamBlock
	{
		private readonly IOpenHourForDate _openHourForDate;
		private readonly ICreateSkillIntervalDataPerDateAndActivity _createSkillIntervalDataPerDateAndActivity;

		public SameOpenHoursInTeamBlock(IOpenHourForDate openHourForDate,
			ICreateSkillIntervalDataPerDateAndActivity createSkillIntervalDataPerDateAndActivity)
		{
			_openHourForDate = openHourForDate;
			_createSkillIntervalDataPerDateAndActivity = createSkillIntervalDataPerDateAndActivity;
		}

		public bool Check(IEnumerable<ISkillDay> allSkillDays, ITeamBlockInfo teamBlockInfo)
		{
			var skillIntervalDataPerDateAndActivity = _createSkillIntervalDataPerDateAndActivity.CreateFor(teamBlockInfo, allSkillDays);

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