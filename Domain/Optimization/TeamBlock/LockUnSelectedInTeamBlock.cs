using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface ILockUnSelectedInTeamBlock
	{
		void Lock(ITeamBlockInfo teamBlockInfo, IList<IPerson> selectedPersons, DateOnlyPeriod selectedPeriod);
	}

	public class LockUnSelectedInTeamBlock : ILockUnSelectedInTeamBlock
	{
		public  void Lock(ITeamBlockInfo teamBlockInfo, IList<IPerson> selectedPersons, DateOnlyPeriod selectedPeriod)
		{
			var blockInfo = teamBlockInfo.BlockInfo;
			blockInfo.ClearLocks();
			var teamInfo = teamBlockInfo.TeamInfo;
			teamInfo.ClearLocks();
			foreach (var dateOnly in blockInfo.BlockPeriod.DayCollection())
			{

				if (!selectedPeriod.Contains(dateOnly))
					blockInfo.LockDate(dateOnly);
			}
			foreach (var groupMember in teamInfo.GroupMembers)
			{
				if (!selectedPersons.Contains(groupMember))
					teamInfo.LockMember(groupMember);
			}
		}
	}
}