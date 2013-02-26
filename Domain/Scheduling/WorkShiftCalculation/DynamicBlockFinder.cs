using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
    public interface IDynamicBlockFinder
    {
        IList<DateOnly> ExtractBlockDays(DateOnly startDateOnly,IGroupPerson groupPerson );

    }

    public class DynamicBlockFinder : IDynamicBlockFinder
    {
        public ISchedulingResultStateHolder SchedulingResultStateHolder { get; set; }
        private readonly ISchedulingOptions _schedulingOptions;
        private readonly IList<IScheduleMatrixPro> _matrixList;
        private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
        private IList<IScheduleMatrixPro> _analyzedMatrix;

        public DynamicBlockFinder(ISchedulingOptions schedulingOptions, ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> matrixList, IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization)
        {
            SchedulingResultStateHolder = schedulingResultStateHolder;
            _schedulingOptions = schedulingOptions;
            _matrixList = matrixList;
            _groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
            _analyzedMatrix = new List<IScheduleMatrixPro>();
        }

        public IList<DateOnly> ExtractBlockDays(DateOnly startDateOnly, IGroupPerson groupPerson1)
        {
            if (_schedulingOptions.BlockFinderTypeForAdvanceScheduling == BlockFinderType.SingleDay)
            {
                return new List<DateOnly> {startDateOnly};
            }
            
            //get the matrix which has the start date
            var validMatrix = new List<IScheduleMatrixPro>();
            foreach (var matrix in _matrixList)
            {
                foreach (var schdaypro in matrix.EffectivePeriodDays)
                {
                    if (schdaypro.Day == startDateOnly)
                    {
                        validMatrix.Add( matrix) ;
                    }
                }
            }

            //create the group person for the matrix
            //var selectedPerson =
            //    validMatrix.Select(scheduleMatrixPro => scheduleMatrixPro.Person).Distinct().ToList();
            //var allGroupPersonListOnStartDate = new HashSet<IGroupPerson>();
            //foreach (var person in selectedPerson)
            //{
            //    allGroupPersonListOnStartDate.Add(_groupPersonBuilderForOptimization.BuildGroupPerson(person, startDateOnly));
            //}
           
            //get the list of matrix from the group person
             var matrixToSelectFrom = new List<IScheduleMatrixPro>();
            //foreach (var randomGroupPerson in allGroupPersonListOnStartDate.GetRandom(allGroupPersonListOnStartDate.Count, true))
            //{
                foreach (var source in validMatrix.Where(x => groupPerson1.GroupMembers.Contains(x.Person) ).ToList())
                {
                    if(_analyzedMatrix.Contains(source)) continue;
                    matrixToSelectFrom.Add(source);
                }
            //}           
               
            //pick a random matrix and add it to processed matrix

            var selectedPeriod = new List<IScheduleDayPro>();
            foreach (var matrix in matrixToSelectFrom.GetRandom(matrixToSelectFrom.Count, true))
            {

                //foreach (var schdaypro in matrix.EffectivePeriodDays)
                //{
                //    if (schdaypro.Day == startDateOnly)
                //    {
                selectedPeriod = new List<IScheduleDayPro>(matrix.EffectivePeriodDays);
                _analyzedMatrix.Add(matrix);
                break;
                //    }
                //}
            }

            var retList = new List<DateOnly>();
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

        private List<DateOnly > GetDaysOffFromSchedule(IEnumerable<DateOnly> dateOnlyListForFullPeriod )
        {
            return (from dateOnly in dateOnlyListForFullPeriod let scheduleDayList = SchedulingResultStateHolder.Schedules.SchedulesForDay(dateOnly) 
                    where scheduleDayList.Any(schedule => schedule.SignificantPart() == SchedulePartView.DayOff) select dateOnly).ToList();
        }
    }
}