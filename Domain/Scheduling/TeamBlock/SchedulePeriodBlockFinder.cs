using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class SchedulePeriodBlockFinder : IBlockFinder
	{
		public IBlockInfo Find(IEnumerable<IScheduleMatrixPro> matrixes, DateOnly blockOnDate, bool singleAgentTeam, bool TEMPTOGGLE)
		{
			if (!TEMPTOGGLE)
				return findRemoveMe(matrixes);

			return new BlockInfoFactory().Execute(singleAgentTeam, matrixes, scheduleMatrixPro => scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod);
		}

		[RemoveMeWithToggle(Toggles.ResourcePlanner_TeamBlockPeriod_42836)]
		private static IBlockInfo findRemoveMe(IEnumerable<IScheduleMatrixPro> matrixes)
		{
			var roleModelMatrix = matrixes.First();
			return new BlockInfo(roleModelMatrix.SchedulePeriod.DateOnlyPeriod);
		}
	}
}