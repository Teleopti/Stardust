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
                            DateOnly startDate = matrixes.First().SchedulePeriod.DateOnlyPeriod.StartDate;
                            while (isAnyTypeOfDayOff(matrixes.First().GetScheduleDayByKey(startDate).DaySchedulePart()))
                            {
                                startDate = startDate.AddDays(1);
                            }
                            foreach (var dateOnly in matrixes.First().SchedulePeriod.DateOnlyPeriod.DayCollection())
                            {
                                if (startDate > dateOnly) continue;
                                if (isAnyTypeOfDayOff(matrixes.First().GetScheduleDayByKey(dateOnly).DaySchedulePart()))
                                {
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