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
	    private readonly IScheduleDayEquator _scheduleDayEquator;

	    public BlockSteadyStateValidator(IScheduleDayEquator scheduleDayEquator)
		{
			_scheduleDayEquator = scheduleDayEquator;
		}

	    public bool IsBlockInSteadyState(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions)
        {
            if (teamBlockInfo == null || schedulingOptions == null) return false ;
            //if (schedulingOptions == null ) throw new ArgumentNullException("schedulingOptions");
            var dayList = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
            if (dayList.Count > 0)
            {
                var matrixList = teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(dayList[0]).ToList();
                if (matrixList.Any())
                {
                    var sampleScheduleDay = matrixList[0].GetScheduleDayByKey(dayList[0]).DaySchedulePart();
                    if (schedulingOptions.UseTeamBlockSameStartTime)
                        return verifySameStartTime(teamBlockInfo, dayList, sampleScheduleDay);
                    if (schedulingOptions.UseTeamBlockSameShift)
                        return verifySameShift(teamBlockInfo, dayList, sampleScheduleDay);
                }
            }
            
            return true;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool IsBlockScheduled(ITeamBlockInfo teamBlockInfo)
        {
            foreach (var day in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
                foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(day))
                    if (!matrix.GetScheduleDayByKey(day).DaySchedulePart().IsScheduled())
                        return false;
            return true;
        }

        private static bool verifySameStartTime(ITeamBlockInfo teamBlockInfo, IEnumerable<DateOnly> dayList, IScheduleDay sampleScheduleDay)
        {
            var dateTimePeriod = getShiftPeriod(sampleScheduleDay);
            if (dateTimePeriod.HasValue)
            {
                var sampleStartTime = dateTimePeriod.Value.StartDateTimeLocal(sampleScheduleDay.TimeZone);
                foreach (var day in dayList)
                    foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(day))
                    {
                        var scheduleDay = matrix.GetScheduleDayByKey(day).DaySchedulePart();
                        if (scheduleDay.IsScheduled() && (scheduleDay.SignificantPart() != SchedulePartView.DayOff) && (scheduleDay.SignificantPart() != SchedulePartView.ContractDayOff) && (scheduleDay.SignificantPart() != SchedulePartView.FullDayAbsence) && (scheduleDay.SignificantPart() != SchedulePartView.Absence ))
                        {
                            var startDateTime = getStartTimeLocal(scheduleDay);
                            //VERIFY IT FROM MICKE
                            if (startDateTime != DateTime.MinValue && startDateTime.TimeOfDay != sampleStartTime.TimeOfDay)
                                return false;
                            
                        }

                    }
            }
            
            return true;
        }

        private static DateTime getStartTimeLocal(IScheduleDay scheduleDay)
        {
            var dateTimePeriod = getShiftPeriod(scheduleDay);
            if (dateTimePeriod.HasValue)
            {
                return dateTimePeriod.Value.StartDateTimeLocal(scheduleDay.TimeZone);
            }
            return DateTime.MinValue ;
        }

        private static DateTimePeriod? getShiftPeriod(IScheduleDay scheduleDay)
        {
            var samplePersonAssignment = scheduleDay.AssignmentHighZOrder();
            if (samplePersonAssignment != null && samplePersonAssignment.ToMainShift()!=null)
            {
                return  samplePersonAssignment.ToMainShift().ProjectionService().CreateProjection().Period();
            }
            return null;
        }

        private bool verifySameShift(ITeamBlockInfo teamBlockInfo, IList<DateOnly> dayList, IScheduleDay sampleScheduleDay)
        {
            foreach (var day in dayList)
                foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(day))
                {
                    var scheduleDay = matrix.GetScheduleDayByKey(day).DaySchedulePart();
                    //scheduleDay.AssignmentHighZOrder().MainShift.ProjectionService().CreateProjection().Period().Value.StartDateTimeLocal(schedulePart.TimeZone)
                    if (scheduleDay.IsScheduled())
                    {
	                    var sourceShift = sampleScheduleDay.GetEditorShift();
	                    var destShift = scheduleDay.GetEditorShift();
						if (sourceShift != null && destShift != null)
							if ((!_scheduleDayEquator.MainShiftEqualsWithoutPeriod(sourceShift, destShift)))
                                return false;
                    }
                       

                }

            return true;
        }
    }
}