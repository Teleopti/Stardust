using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface IDynamicBlockFinder
    {
	    IBlockInfo ExtractBlockInfo(DateOnly blockOnDate, ITeamInfo teamInfo, IBlockFinder blockFinder, bool singleAgentTeam);
    }

    public class DynamicBlockFinderOLD : IDynamicBlockFinder
    {
	    public IBlockInfo ExtractBlockInfo(DateOnly blockOnDate, ITeamInfo teamInfo, IBlockFinder blockFinder, bool singleAgentTeam)
	    {
		    IEnumerable<IScheduleMatrixPro> tempMatrixes = teamInfo.MatrixesForGroupAndDate(blockOnDate).ToList();
		    if (!tempMatrixes.Any())
			    return null;

		    return blockFinder.Find(tempMatrixes, blockOnDate, singleAgentTeam, false);
	    }
    }


	public class DynamicBlockFinder : IDynamicBlockFinder
	{
		public IBlockInfo ExtractBlockInfo(DateOnly blockOnDate, ITeamInfo teamInfo, IBlockFinder blockFinder, bool singleAgentTeam)
		{
			IEnumerable<IScheduleMatrixPro> tempMatrixes = teamInfo.MatrixesForGroupAndDate(blockOnDate).ToList();
			if (!tempMatrixes.Any())
				return null;

			return blockFinder.Find(tempMatrixes, blockOnDate, singleAgentTeam, true);
		}
	}
}