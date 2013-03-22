using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{

    public interface IBlockSteadyStateValidator
    {
        bool IsBlockInSteadyState(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions);
        bool IsBlockScheduled(ITeamBlockInfo teamBlockInfo);
    }

    public class BlockSteadyStateValidator : IBlockSteadyStateValidator
    {
        public bool IsBlockInSteadyState(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions)
        {
            var dayList = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
            if (dayList.Count > 0)
            {
                var matrixList = teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(dayList[0]).ToList();
                if (matrixList.Any())
                {
                    var sampleScheduleDay = matrixList[0].GetScheduleDayByKey(dayList[0]).DaySchedulePart();
                    if (schedulingOptions.UseLevellingSameStartTime)
                        return verifySameStartTime(teamBlockInfo, dayList, sampleScheduleDay);
                    if (schedulingOptions.UseLevellingSameShift)
                        return verifySameShift(teamBlockInfo, dayList, sampleScheduleDay);
                }
            }
            
            return true;
        }

        public bool IsBlockScheduled(ITeamBlockInfo teamBlockInfo)
        {
            foreach (var day in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
                foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(day))
                    if (!matrix.GetScheduleDayByKey(day).DaySchedulePart().IsScheduled())
                        return false;
            return true;
        }

        private bool verifySameStartTime(ITeamBlockInfo teamBlockInfo, IEnumerable<DateOnly> dayList, IScheduleDay sampleScheduleDay)
        {
            var dateTimePeriod = getShiftPeriod(sampleScheduleDay);
            if (dateTimePeriod.HasValue)
            {
                var sampleStartTime = dateTimePeriod.Value.StartDateTimeLocal(sampleScheduleDay.TimeZone);
                foreach (var day in dayList)
                    foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(day))
                    {
                        var scheduleDay = matrix.GetScheduleDayByKey(day).DaySchedulePart();
                        if (scheduleDay.IsScheduled())
                        {
                            var startDateTime = getStartTimeLocal(scheduleDay);
                            if (startDateTime != sampleStartTime)
                                return false;
                        }

                    }
            }
            
            return true;
        }

        private DateTime getStartTimeLocal(IScheduleDay scheduleDay)
        {
            var dateTimePeriod = getShiftPeriod(scheduleDay);
            if (dateTimePeriod.HasValue)
            {
                return dateTimePeriod.Value.StartDateTimeLocal(scheduleDay.TimeZone);
            }
            return DateTime.MinValue ;
        }

        private DateTimePeriod? getShiftPeriod(IScheduleDay scheduleDay)
        {
            var samplePersonAssignment = scheduleDay.AssignmentHighZOrder();
            if (samplePersonAssignment != null && samplePersonAssignment.MainShift!=null)
            {
                return  samplePersonAssignment.MainShift.ProjectionService().CreateProjection().Period();
            }
            return null;
        }

        private bool verifySameShift(ITeamBlockInfo teamBlockInfo, IList<DateOnly> dayList, IScheduleDay sampleScheduleDay)
        {
            var equator = new ScheduleDayEquator();
            foreach (var day in dayList)
                foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(day))
                {
                    var scheduleDay = matrix.GetScheduleDayByKey(day).DaySchedulePart();
                    //scheduleDay.AssignmentHighZOrder().MainShift.ProjectionService().CreateProjection().Period().Value.StartDateTimeLocal(schedulePart.TimeZone)
                    if (scheduleDay.IsScheduled())
                        if (!equator.MainShiftEquals(sampleScheduleDay, scheduleDay))
                            return false;

                }

            return true;
        }
    }
}