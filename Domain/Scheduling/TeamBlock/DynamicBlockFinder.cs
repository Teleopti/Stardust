using System;
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public IBlockInfo ExtractBlockInfo(DateOnly blockOnDate, ITeamInfo teamInfo, BlockFinderType blockType)
	    {
	        DateOnlyPeriod? blockPeriod = null;
		    if (blockType == BlockFinderType.None)
			    return null;

			IEnumerable<IScheduleMatrixPro> tempMatrixes = teamInfo.MatrixesForGroupAndDate(blockOnDate).ToList();
            if (!tempMatrixes.Any())
			    return null;

            IScheduleMatrixPro roleModelMatrix = tempMatrixes.First();
            
		    switch (blockType)
		    {
			    case BlockFinderType.SingleDay:
				    {
					    blockPeriod = new DateOnlyPeriod(blockOnDate, blockOnDate);
					    break;
				    }

				case BlockFinderType.SchedulePeriod:
				    {
                        blockPeriod = roleModelMatrix.SchedulePeriod.DateOnlyPeriod;
					    break;
				    }

				case BlockFinderType.BetweenDayOff:
		            {
		                var scheduleDayProTemp = tempMatrixes.First().GetScheduleDayByKey(blockOnDate);
                        if (scheduleDayProTemp != null)
                        {
                            if (isDayOff(tempMatrixes.First().GetScheduleDayByKey(blockOnDate).DaySchedulePart())) return null;
                        }

		                var blockPeriodFinderBetweenDayOff = new BlockPeriodFinderBetweenDayOff();
		                blockPeriod = blockPeriodFinderBetweenDayOff.GetBlockPeriod(tempMatrixes.First(), blockOnDate);
                        
                        break;

				    }
		    }

		    if (!blockPeriod.HasValue)
		    {
			    return null;
		    }

			return new BlockInfo(blockPeriod.Value);
	    }

        //Absence can not be a block breaker when using teams
        private static bool isDayOff(IScheduleDay scheduleDay)
        {
            var significantPart = scheduleDay.SignificantPart();
            if (significantPart == SchedulePartView.DayOff ||
                                   significantPart == SchedulePartView.ContractDayOff)
            {
                return true;
            }
            return false;
        }
    }
}