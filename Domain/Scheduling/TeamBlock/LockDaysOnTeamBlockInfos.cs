using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class LockDaysOnTeamBlockInfos
	{
		public void LockUnscheduleDaysAndRemoveEmptyTeamBlockInfos(ICollection<ITeamBlockInfo> teamBlockInfos)
		{
			foreach (var teamBlockInfo in teamBlockInfos.ToArray())
			{
				var anyIsScheduled = false;
				foreach (var scheduleMatrixPro in teamBlockInfo.MatrixesForGroupAndBlock())
				{
					foreach (var scheduleDayPro in scheduleMatrixPro.UnlockedDays)
					{
						var scheduleDay = scheduleDayPro.DaySchedulePart();
						var dateOnly = scheduleDayPro.Day;
						var person = scheduleDay.Person;

						if (!teamBlockInfo.TeamInfo.UnLockedMembers(dateOnly).Contains(person))
							continue;
						if (scheduleDay.IsScheduled())
						{
							anyIsScheduled = true;
						}
						else
						{
							teamBlockInfo.TeamInfo.LockMember(dateOnly.ToDateOnlyPeriod(), person);
						}
					}
				}
				if (!anyIsScheduled)
				{
					teamBlockInfos.Remove(teamBlockInfo);
				}
			}
		}
	}
}