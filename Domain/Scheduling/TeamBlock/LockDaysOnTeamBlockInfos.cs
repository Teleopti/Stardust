using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class LockDaysOnTeamBlockInfos
	{
		public void LockUnscheduleDaysAndRemoveEmptyTeamBlockInfos(ICollection<ITeamBlockInfo> teamBlockInfos)
		{
			foreach (var teamBlockInfo in teamBlockInfos.ToArray())
			{
				var anyScheduled = false;
				var unlockedDates = teamBlockInfo.BlockInfo.UnLockedDates();

				foreach (var unlockedDate in unlockedDates)
				{
					var unlockedPersons = teamBlockInfo.TeamInfo.UnLockedMembers(unlockedDate);

					foreach (var person in unlockedPersons)
					{
						var scheduleDay = teamBlockInfo.TeamInfo.MatrixForMemberAndDate(person, unlockedDate)
							.GetScheduleDayByKey(unlockedDate)
							.DaySchedulePart();
						if (scheduleDay.IsScheduled())
						{
							anyScheduled = true;
						}
						else
						{
							teamBlockInfo.TeamInfo.LockMember(unlockedDate.ToDateOnlyPeriod(), person);
						}
					}
				}
				if (!anyScheduled)
				{
					teamBlockInfos.Remove(teamBlockInfo);
				}
			}
		}
	}
}