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
            IEnumerable<IScheduleMatrixPro> matrixesToVerify = teamInfo.MatrixesForGroupAndDate(blockOnDate).ToList();
            if (matrixesToVerify.Any())
                if (isAnyTypeOfDayOff(matrixesToVerify.First().GetScheduleDayByKey(blockOnDate).DaySchedulePart())) return null;

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
                        //THE DATE POINTERT SHOULD NEVER BE A DAYOFF
                        IEnumerable<IScheduleMatrixPro> matrixes = teamInfo.MatrixesForGroupAndDate(blockOnDate).ToList();
                        if (matrixes.Any())
                        {
                            //move to left side to get the starting date
                            DateOnly startDate = blockOnDate;
                            while (!isAnyTypeOfDayOff((matrixes.First().GetScheduleDayByKey(startDate).DaySchedulePart())))
                            {
                                startDate = startDate.AddDays(-1);
                            }
                            startDate = startDate.AddDays(1);
                            
                            foreach (var dateOnly in matrixes.First().SchedulePeriod.DateOnlyPeriod.DayCollection())
                            {
                                if (startDate > dateOnly) continue;
                                if (isAnyTypeOfDayOff(matrixes.First().GetScheduleDayByKey(dateOnly).DaySchedulePart()))
                                {
                                    blockPeriod = new DateOnlyPeriod(startDate,dateOnly.AddDays(-1));
                                    break;
                                }
                                if (dateOnly == matrixes.First().SchedulePeriod.DateOnlyPeriod.EndDate)
                                {
                                    blockPeriod = new DateOnlyPeriod(startDate, dateOnly);
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