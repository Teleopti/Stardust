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

                        ////THE DATE POINTERT SHOULD NEVER BE A DAYOFF
                        ////IEnumerable<IScheduleMatrixPro> matrixes = teamInfo.MatrixesForGroupAndDate(blockOnDate).ToList();
                        ////if (matrixes.Any())
                        ////{
                        //    //move to left side to get the starting date
                        //DateOnly startDate = blockOnDate;
                        //scheduleDayProTemp = tempMatrixes.First().GetScheduleDayByKey(startDate);
                        //while (scheduleDayProTemp != null && !isDayOff(scheduleDayProTemp.DaySchedulePart()))
                        //{
                        //    startDate = startDate.AddDays(-1);
                        //    scheduleDayProTemp = tempMatrixes.First().GetScheduleDayByKey(startDate);
                        //}
                        //startDate = startDate.AddDays(1);

                        //foreach (var dateOnly in tempMatrixes.First().SchedulePeriod.DateOnlyPeriod.DayCollection())
                        //{
                        //    if (startDate > dateOnly) continue;

                        //    scheduleDayProTemp = tempMatrixes.First().GetScheduleDayByKey(dateOnly);
                        //    if (scheduleDayProTemp == null) continue;

                        //    if (isDayOff(scheduleDayProTemp.DaySchedulePart()))
                        //    {
                        //        blockPeriod = new DateOnlyPeriod(startDate, dateOnly.AddDays(-1));
                        //        break;
                        //    }
                        //    if (dateOnly == tempMatrixes.First().SchedulePeriod.DateOnlyPeriod.EndDate)
                        //    {
                        //        blockPeriod = new DateOnlyPeriod(startDate, dateOnly);
                        //        break;
                        //    }


                        //}
                        //}
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