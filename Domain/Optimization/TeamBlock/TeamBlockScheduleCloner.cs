using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public class TeamBlockScheduleCloner
	{
		public IEnumerable<IScheduleDay> CloneSchedules(ITeamBlockInfo teamBlock)
		{
			var clonedSchedules = new List<IScheduleDay>();
			var teamInfo = teamBlock.TeamInfo;
			foreach (var groupMember in teamInfo.GroupMembers)
			{
				var scheduleMatrixPro = teamInfo.MatrixForMemberAndDate(groupMember, teamBlock.BlockInfo.BlockPeriod.StartDate);
				if (scheduleMatrixPro == null) continue;
				var rangeForMember = scheduleMatrixPro.ActiveScheduleRange;
				clonedSchedules.AddRange(rangeForMember.ScheduledDayCollection(teamBlock.BlockInfo.BlockPeriod).Select(s => (IScheduleDay)s.Clone()));
			}

			return clonedSchedules;
		} 
	}
}