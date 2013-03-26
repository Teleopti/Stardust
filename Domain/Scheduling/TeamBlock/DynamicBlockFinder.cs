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

	    public IBlockInfo ExtractBlockInfo(DateOnly blockOnDate, ITeamInfo teamInfo, BlockFinderType blockType)
	    {
	        if (teamInfo == null) throw new ArgumentNullException("teamInfo");
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
                case BlockFinderType.BetweenDayOff :
		            {
                        IEnumerable<IScheduleMatrixPro> matrixes = teamInfo.MatrixesForGroupAndDate(blockOnDate).ToList();
                        if (matrixes.Any())
                        {
                            //getting the start date of the (what if the start date is a dayoff
                            DateOnly startDate = matrixes.First().SchedulePeriod.DateOnlyPeriod.StartDate;
                            foreach (var dateOnly in matrixes.First().SchedulePeriod.DateOnlyPeriod.DayCollection())
                            {
                                if (isAnyTypeOfDayOff(matrixes.First().GetScheduleDayByKey(dateOnly).DaySchedulePart()))
                                {
                                    //TODO: should we include the DO in the period?
                                    blockPeriod = new DateOnlyPeriod(startDate,dateOnly.AddDays(-1));
                                    break;
                                }
                                    
                            }
                        }
                        break;
		            }

		    }

		    if (!blockPeriod.HasValue)
		    {
			    return null;
		    }

			return new BlockInfo(blockPeriod.Value);
	    }

        private bool isAnyTypeOfDayOff(IScheduleDay scheduleDay)
        {
            var significantPart = scheduleDay.SignificantPart();
            if (significantPart == SchedulePartView.DayOff ||
                                   significantPart == SchedulePartView.ContractDayOff ||
                                   significantPart == SchedulePartView.Absence)
            {
                return true;
            }
            return false;
        }
    }
}