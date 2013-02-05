using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
    public interface IDynamicBlockFinder
    {
        List<DateOnly> ExtractBlockDays(DateOnly startDateOnly);

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

        public List<DateOnly> ExtractBlockDays(DateOnly startDateOnly)
        {
            var selectedPeriod = new List<IScheduleDayPro>();
            foreach (var matrix in _matrixList)
            {
                foreach (var schdaypro in matrix.EffectivePeriodDays)
                {
                    if (schdaypro.Day == startDateOnly)
                    {
                        selectedPeriod = new List<IScheduleDayPro>(matrix.EffectivePeriodDays);
                        break;
                    }
                }
            }

            var retList = new List<DateOnly>();
            if (_schedulingOptions.BlockFinderTypeForAdvanceScheduling == BlockFinderType.SchedulePeriod)
            {
                retList = selectedPeriod.Select(s => s.Day).ToList() ;
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


            return retList;
            
            
        }

        private List<DateOnly > GetDaysOffFromSchedule(IEnumerable<DateOnly> dateOnlyListForFullPeriod )
        {
            return (from dateOnly in dateOnlyListForFullPeriod let scheduleDayList = SchedulingResultStateHolder.Schedules.SchedulesForDay(dateOnly) 
                    where scheduleDayList.Any(schedule => schedule.SignificantPart() == SchedulePartView.DayOff) select dateOnly).ToList();
        }
    }
}