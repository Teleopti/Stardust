using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
    public interface IDynamicBlockFinder
    {
        IList<DateOnly> ExtractBlockDays(DateOnly startDateOnly,IGroupPerson groupPerson );
	    BlockInfo ExtractBlockInfo(DateOnly blockOnDate, ITeamInfo teamInfo, BlockFinderType blockType);

    }

    public class DynamicBlockFinder : IDynamicBlockFinder
    {
        public ISchedulingResultStateHolder SchedulingResultStateHolder { get; set; }
        private readonly ISchedulingOptions _schedulingOptions;
        private readonly IList<IScheduleMatrixPro> _matrixList;

        public DynamicBlockFinder(ISchedulingOptions schedulingOptions, ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> matrixList)
        {
            SchedulingResultStateHolder = schedulingResultStateHolder;
            _schedulingOptions = schedulingOptions;
            _matrixList = matrixList;
        }

        public IList<DateOnly> ExtractBlockDays(DateOnly startDateOnly, IGroupPerson groupPerson1)
        {
            if (_schedulingOptions.BlockFinderTypeForAdvanceScheduling == BlockFinderType.SingleDay)
            {
                return new List<DateOnly> {startDateOnly};
            }
            
            //get the matrix which has the start date
            var validMatrix = new List<IScheduleMatrixPro>();
            foreach (var matrix in _matrixList.Where(x=>groupPerson1.GroupMembers.Contains(x.Person)))
            {
                foreach (var schdaypro in matrix.EffectivePeriodDays  )
                {
                    
                    if (schdaypro.Day == startDateOnly  && !matrix.GetScheduleDayByKey(startDateOnly).DaySchedulePart().IsScheduled() )
                    {
                        validMatrix.Add( matrix) ;
                    }
                }
            }

           
            var retList = new List<DateOnly>();
            
            if (validMatrix.Count == 0) return retList;
            var selectedPeriod = new List<IScheduleDayPro>(validMatrix[0] .EffectivePeriodDays);
            if (_schedulingOptions.BlockFinderTypeForAdvanceScheduling == BlockFinderType.SchedulePeriod)
            {
                retList = selectedPeriod.Select(s => s.Day).ToList();
            }
            else if (_schedulingOptions.BlockFinderTypeForAdvanceScheduling == BlockFinderType.Weeks)
            {
                var dateOnlyList = new List<DateOnly>();
                foreach (var dateOnly in selectedPeriod.Select(s => s.Day).ToList())
                {
                    if (dateOnly.DayOfWeek.Equals(DayOfWeek.Sunday) || dateOnly.DayOfWeek.Equals(DayOfWeek.Saturday))
                    {
                        break;
                    }
                    dateOnlyList.Add(dateOnly);
                }

                retList = dateOnlyList.ToList();
            }
            else if (_schedulingOptions.BlockFinderTypeForAdvanceScheduling == BlockFinderType.BetweenDayOff)
            {
                var dateOnlyList = selectedPeriod.Select(s => s.Day).ToList();
                var dayOffList = GetDaysOffFromSchedule(dateOnlyList);
                var extractedBlock = new List<DateOnly>();
                for (int i = 0; i < dateOnlyList.Count; i++)
                {
                    if (i + 1 < dateOnlyList.Count && dayOffList.Contains(dateOnlyList[i]) && dayOffList.Contains(dateOnlyList[i]))
                    {
                        break;
                    }
                    extractedBlock.Add(dateOnlyList[i]);
                }
                retList = extractedBlock.ToList();
            }
            
            return  retList;
            }

	    public BlockInfo ExtractBlockInfo(DateOnly blockOnDate, ITeamInfo teamInfo, BlockFinderType blockType)
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
					    blockPeriod = teamInfo.GroupPerson.GroupMembers[0].VirtualSchedulePeriod(blockOnDate).DateOnlyPeriod;
					    break;
				    }
		    }

		    if (!blockPeriod.HasValue)
		    {
			    return null;
		    }

			return new BlockInfo(blockPeriod.Value);
	    }

	    private List<DateOnly > GetDaysOffFromSchedule(IEnumerable<DateOnly> dateOnlyListForFullPeriod )
        {
            return (from dateOnly in dateOnlyListForFullPeriod let scheduleDayList = SchedulingResultStateHolder.Schedules.SchedulesForDay(dateOnly) 
                    where scheduleDayList.Any(schedule => schedule.SignificantPart() == SchedulePartView.DayOff) select dateOnly).ToList();
        }
    }

	
}