using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval
{
	public interface ITeamBlockOpenHoursValidator
	{
		bool Validate(ITeamBlockInfo teamBlockInfo, ISchedulingResultStateHolder schedulingResultStateHolder);
	}

	public class TeamBlockOpenHoursValidator : ITeamBlockOpenHoursValidator
	{
		private readonly CreateSkillIntervalDataPerDateAndActivity _createSkillIntervalDataPerDateAndActivity;
		private readonly ISkillIntervalDataOpenHour _skillIntervalDataOpenHour;
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;

		public TeamBlockOpenHoursValidator(CreateSkillIntervalDataPerDateAndActivity createSkillIntervalDataPerDateAndActivity, ISkillIntervalDataOpenHour skillIntervalDataOpenHour, IGroupPersonSkillAggregator groupPersonSkillAggregator)
		{
			_createSkillIntervalDataPerDateAndActivity = createSkillIntervalDataPerDateAndActivity;
			_skillIntervalDataOpenHour = skillIntervalDataOpenHour;
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
		}

		public bool Validate(ITeamBlockInfo teamBlockInfo, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			var agentTimeZoneInfo = teamBlockInfo.TeamInfo.GroupMembers.First().PermissionInformation.DefaultTimeZone();
			var dayIntervalDataPerDateAndActivity = _createSkillIntervalDataPerDateAndActivity.CreateForAgent(
				teamBlockInfo, schedulingResultStateHolder.SkillDays.ToSkillDayEnumerable(),
				_groupPersonSkillAggregator, agentTimeZoneInfo);
			var firstDate = findFirstNoneDayOffOrFullDayAbsenceDayForAnyTeamMember(teamBlockInfo.BlockInfo.BlockPeriod,
				teamBlockInfo.TeamInfo.GroupMembers.ToList(), schedulingResultStateHolder);
			var firstDateActivities = dayIntervalDataPerDateAndActivity[firstDate].Keys.ToList();
			

			foreach (var activity in firstDateActivities)
			{
				var dictionary = dayIntervalDataPerDateAndActivity[firstDate];
				var firstDateOpenPeriod = _skillIntervalDataOpenHour.GetOpenHours(dictionary[activity], firstDate);

				foreach (var dateOnly in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
				{
					if(dateOnly.Equals(firstDate)) continue;

					if (!isDayOffOrFullAbsenceForAllTeamMembers(dateOnly, teamBlockInfo.TeamInfo.GroupMembers, schedulingResultStateHolder))
					{
						dictionary = dayIntervalDataPerDateAndActivity[dateOnly];
					var openPeriod = _skillIntervalDataOpenHour.GetOpenHours(dictionary[activity], dateOnly);

					if (openPeriod != firstDateOpenPeriod) return false;
					}
				}
			}

			return true;
		}

		private DateOnly findFirstNoneDayOffOrFullDayAbsenceDayForAnyTeamMember(DateOnlyPeriod period, IList<IPerson> teamMembers , ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			var dayCollection = period.DayCollection();
			foreach (var dateOnly in dayCollection)
			{
				if (!isDayOffOrFullAbsenceForAllTeamMembers(dateOnly, teamMembers, schedulingResultStateHolder))
					return dateOnly;
			}

			return dayCollection.First();
		}

		private bool isDayOffOrFullAbsenceForAllTeamMembers(DateOnly dateOnly, IEnumerable<IPerson> teamMembers, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			bool dayOffOrAbsenceFound = false;
			foreach (var groupMember in teamMembers)
			{
				var scheduleDay = schedulingResultStateHolder.Schedules[groupMember].ScheduledDay(dateOnly);
				if (scheduleDay.HasDayOff() || scheduleDay.IsFullDayAbsence())
				{
					dayOffOrAbsenceFound = true;
					break;
				}
			}

			return dayOffOrAbsenceFound;
		}
	}
}
