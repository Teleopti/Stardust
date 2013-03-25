using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface IDynamicBlockFinder
    {
	    IBlockInfo ExtractBlockInfo(DateOnly blockOnDate, ITeamInfo teamInfo, BlockFinderType blockType);
    }

    public class DynamicBlockFinder : IDynamicBlockFinder
    {

	    public IBlockInfo ExtractBlockInfo(DateOnly blockOnDate, ITeamInfo teamInfo, BlockFinderType blockType)
	    {
		    DateOnlyPeriod? blockPeriod = null;
		    switch (blockType)
		    {
			        case BlockFinderType.SingleDay:
				    {
					    blockPeriod = new DateOnlyPeriod(blockOnDate, blockOnDate);
					    break;
				    }

					case BlockFinderType.SchedulePeriod:
				    {
						IEnumerable<IScheduleMatrixPro> matrixes = teamInfo.MatrixesForGroupAndDate(blockOnDate).ToList();
					    if (matrixes.Any())
						    blockPeriod = matrixes.First().SchedulePeriod.DateOnlyPeriod;
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