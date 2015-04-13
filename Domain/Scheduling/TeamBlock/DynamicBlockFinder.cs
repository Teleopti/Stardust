using System.Collections.Generic;
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

		    IEnumerable<IScheduleMatrixPro> tempMatrixes = teamInfo.MatrixesForGroupAndDate(blockOnDate).ToList();
		    if (!tempMatrixes.Any())
			    return null;

		    IScheduleMatrixPro roleModelMatrix = tempMatrixes.First();

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
				    blockPeriod = roleModelMatrix.SchedulePeriod.DateOnlyPeriod;
				    break;
			    }

			    case BlockFinderType.BetweenDayOff:
			    {
				    var blockPeriodFinderBetweenDayOff = new BlockPeriodFinderBetweenDayOff();
					blockPeriod = blockPeriodFinderBetweenDayOff.GetBlockPeriod(roleModelMatrix, blockOnDate, singleAgentTeam);
				    break;
			    }
		    }

		    if (!blockPeriod.HasValue)
		    {
			    return null;
		    }

		    return new BlockInfo(blockPeriod.Value);
	    }
    }
}