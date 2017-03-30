using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IBlockFinder
	{
		[RemoveMeWithToggle("remove temptoggle param when toggle is removed", Toggles.ResourcePlanner_TeamBlockPeriod_42836)]
		IBlockInfo Find(IEnumerable<IScheduleMatrixPro> matrixes, DateOnly blockOnDate, bool singleAgentTeam, bool TEMPTOGGLE);
	}
}