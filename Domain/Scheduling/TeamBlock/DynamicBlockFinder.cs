using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface IDynamicBlockFinder
    {
	    IBlockInfo ExtractBlockInfo(DateOnly blockOnDate, ITeamInfo teamInfo, BlockFinderType blockType, bool singleAgentTeam);
    }

    public class DynamicBlockFinder : IDynamicBlockFinder
    {
	    public IBlockInfo ExtractBlockInfo(DateOnly blockOnDate, ITeamInfo teamInfo, BlockFinderType blockType,
		    bool singleAgentTeam)
	    {
		    DateOnlyPeriod? blockPeriod = null;

		    var tempMatrixes = teamInfo.MatrixesForGroupAndDate(blockOnDate).ToList();
		    if (!tempMatrixes.Any())
			    return null;

		    switch (blockType)
		    {
			    case BlockFinderType.SingleDay:
			    {
				    blockPeriod = new DateOnlyPeriod(blockOnDate, blockOnDate);
					var periodOk = false;
					foreach (var matrix in tempMatrixes)
				    {
						var scheduleDay = matrix.GetScheduleDayByKey(blockOnDate).DaySchedulePart();
					    if (!scheduleDay.HasDayOff())
					    {
						    periodOk = true;
							break;
					    }
				    }
					if (!periodOk)
					    blockPeriod = null;

				    break;
			    }

			    case BlockFinderType.SchedulePeriod:
			    {
					var roleModelMatrix = tempMatrixes[0];
				    blockPeriod = roleModelMatrix.SchedulePeriod.DateOnlyPeriod;
				    break;
			    }

			    case BlockFinderType.BetweenDayOff:
				{
					var roleModelMatrix = tempMatrixes[0];
				    var blockPeriodFinderBetweenDayOff = new BlockPeriodFinderBetweenDayOff();
					blockPeriod = blockPeriodFinderBetweenDayOff.GetBlockPeriod(roleModelMatrix, blockOnDate, singleAgentTeam);
				    break;
			    }
		    }

		    return !blockPeriod.HasValue ? null : new BlockInfo(blockPeriod.Value);
	    }
    }
}