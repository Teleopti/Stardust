using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
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
					    blockPeriod = teamInfo.MatrixesForGroup.FirstOrDefault().SchedulePeriod.DateOnlyPeriod;
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