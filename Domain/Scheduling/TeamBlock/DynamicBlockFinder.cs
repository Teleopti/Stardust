using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface IDynamicBlockFinder
    {
	    IBlockInfo ExtractBlockInfo(DateOnly blockOnDate, ITeamInfo teamInfo, IBlockFinder blockFinder);
    }

    public class DynamicBlockFinder : IDynamicBlockFinder
    {
	    private readonly bool _resourcePlannerTeamBlockPeriod42836Hack;

		[RemoveMeWithToggle(Toggles.ResourcePlanner_TeamBlockPeriod_42836)]
		public DynamicBlockFinder(bool resourcePlannerTeamBlockPeriod42836_Hack)
	    {
		    _resourcePlannerTeamBlockPeriod42836Hack = resourcePlannerTeamBlockPeriod42836_Hack;
	    }

	    public IBlockInfo ExtractBlockInfo(DateOnly blockOnDate, ITeamInfo teamInfo, IBlockFinder blockFinder)
	    {
		    IEnumerable<IScheduleMatrixPro> tempMatrixes = teamInfo.MatrixesForGroupAndDate(blockOnDate).ToList();
		    if (!tempMatrixes.Any())
			    return null;

		    return blockFinder.Find(tempMatrixes, blockOnDate, _resourcePlannerTeamBlockPeriod42836Hack);
	    }
    }
}