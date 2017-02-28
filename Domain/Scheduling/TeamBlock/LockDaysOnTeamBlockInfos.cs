using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
						var scheduleMatrixPro = teamBlockInfo.TeamInfo.MatrixForMemberAndDate(person, unlockedDate);
						if (scheduleMatrixPro == null) continue;
						var scheduleDay = scheduleMatrixPro.GetScheduleDayByKey(unlockedDate).DaySchedulePart();
						if (scheduleDay.SignificantPart() == SchedulePartView.MainShift)
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