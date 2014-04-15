

using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface ITeamBlockScheduleCloner
	{
		IEnumerable<IScheduleDay> CloneSchedules(ITeamBlockInfo teamBlock);
	}

	public class TeamBlockScheduleCloner : ITeamBlockScheduleCloner
	{
		public IEnumerable<IScheduleDay> CloneSchedules(ITeamBlockInfo teamBlock)
		{
			IList<IScheduleDay> clonedSchedules = new List<IScheduleDay>();
			var dates = teamBlock.BlockInfo.BlockPeriod.DayCollection();
			var teamInfo = teamBlock.TeamInfo;
			foreach (var groupMember in teamInfo.GroupMembers)
			{
				var rangeForMember =
					teamInfo.MatrixForMemberAndDate(groupMember, dates.First()).ActiveScheduleRange;
				foreach (var dateOnly in dates)
				{
					clonedSchedules.Add((IScheduleDay)rangeForMember.ScheduledDay(dateOnly).Clone());
				}
			}

			return clonedSchedules;
		} 
	}
}