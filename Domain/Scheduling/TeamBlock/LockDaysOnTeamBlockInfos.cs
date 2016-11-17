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

						//CLAES! konstigt - detta block behövs inte längre? Varför behöver vi inte längre låsa oschemalagda dagar?
						//Om denna verkligen kan tas bort - renama filen till nåt annat då den inte längre låser scheman utan bara tar bort onödiga teamblocks

						//else
						//{
						//	teamBlockInfo.TeamInfo.LockMember(unlockedDate.ToDateOnlyPeriod(), person);
						//}
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