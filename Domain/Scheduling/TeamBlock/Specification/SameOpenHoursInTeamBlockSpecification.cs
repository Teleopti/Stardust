using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification
{
	public interface ISameOpenHoursInTeamBlockSpecification
	{
		bool IsSatisfiedBy(ITeamBlockInfo skillDays);
	}

	public class SameOpenHoursInTeamBlockSpecification : Specification<ITeamBlockInfo>,
		ISameOpenHoursInTeamBlockSpecification
	{
		private readonly IOpenHourForDate _openHourForDate;
		private readonly ICreateSkillIntervalDataPerDateAndActivity _createSkillIntervalDataPerDateAndActivity;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;

		public SameOpenHoursInTeamBlockSpecification(IOpenHourForDate openHourForDate,
			ICreateSkillIntervalDataPerDateAndActivity
				createSkillIntervalDataPerDateAndActivity,
			ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_openHourForDate = openHourForDate;
			_createSkillIntervalDataPerDateAndActivity = createSkillIntervalDataPerDateAndActivity;
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		public override bool IsSatisfiedBy(ITeamBlockInfo teamBlockInfo)
		{
			var skillIntervalDataPerDateAndActivity = _createSkillIntervalDataPerDateAndActivity.CreateFor(teamBlockInfo,
				_schedulingResultStateHolder);

			var dates = skillIntervalDataPerDateAndActivity.Keys;
			var sampleDate = dates.First();
			var sampleOpenHour = _openHourForDate.OpenHours(sampleDate, skillIntervalDataPerDateAndActivity[sampleDate]);
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