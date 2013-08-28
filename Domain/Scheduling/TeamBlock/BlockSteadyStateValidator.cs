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
            var dayList = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
            if (dayList.Count > 0)
            {
                var matrixList = teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(dayList[0]).ToList();
                if (matrixList.Any())
                {
                    var sampleScheduleDay = getValidSampleDay(matrixList[0],dayList);
                    if (sampleScheduleDay == null) return true;
                    if (schedulingOptions.UseTeamBlockSameStartTime)
                        return verifySameStartTime(teamBlockInfo, dayList, sampleScheduleDay);
                    if (schedulingOptions.UseTeamBlockSameShift)
                        return verifySameShift(teamBlockInfo, dayList, sampleScheduleDay);
                    if(schedulingOptions.UseTeamBlockSameEndTime )
                        return verifySameEndTime(teamBlockInfo, dayList, sampleScheduleDay);
                    if(schedulingOptions.UseTeamBlockSameShiftCategory  )
                        return verifySameShiftCategory(teamBlockInfo, dayList, sampleScheduleDay);
                }
            }
            
            return true;
        }

        private IScheduleDay getValidSampleDay(IScheduleMatrixPro matrix, IList<DateOnly> dayList)
        {
            foreach (var dateOnly in dayList)
            {
                var scheduleDay = matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart();
                if (isValidScheduleDay(scheduleDay  ))
                    return scheduleDay;
            }
            return null;
        }

        private static bool isValidScheduleDay(IScheduleDay scheduleDay)
        {
            return scheduleDay.IsScheduled() && (scheduleDay.SignificantPart() != SchedulePartView.DayOff) &&
                   (scheduleDay.SignificantPart() != SchedulePartView.ContractDayOff) &&
                   (scheduleDay.SignificantPart() != SchedulePartView.FullDayAbsence) &&
                   (scheduleDay.SignificantPart() != SchedulePartView.Absence);
        }

        private bool verifySameShiftCategory(ITeamBlockInfo teamBlockInfo, IList<DateOnly> dayList, IScheduleDay sampleScheduleDay)
        {
            var sampleShiftCategory = getShiftCategory(sampleScheduleDay);
            if (sampleShiftCategory != null)
            {
                foreach (var day in dayList)
                    foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(day))
                    {
                        var scheduleDay = matrix.GetScheduleDayByKey(day).DaySchedulePart();
                        if (isValidScheduleDay(scheduleDay))
                        {
                            var shiftCategory = getShiftCategory(scheduleDay);
                            if (shiftCategory != null)
                            {
                                if (shiftCategory!=sampleShiftCategory)
                                    return false;
                            }

                        }

                    }
            }
            return true;
        }
        
        private bool verifySameEndTime(ITeamBlockInfo teamBlockInfo, IList<DateOnly> dayList, IScheduleDay sampleScheduleDay)
        {
            var dateTimePeriod = getShiftPeriod(sampleScheduleDay.GetEditorShift());
            if (dateTimePeriod.HasValue)
            {
                var sampleEndTime = dateTimePeriod.Value.EndDateTimeLocal(sampleScheduleDay.TimeZone);
                foreach (var day in dayList)
                    foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(day))
                    {
                        var scheduleDay = matrix.GetScheduleDayByKey(day).DaySchedulePart();
                        if (isValidScheduleDay(scheduleDay ))
                        {
                            var endDateTime = getEndTimeLocal(scheduleDay);
                            if (endDateTime != DateTime.MinValue && endDateTime.TimeOfDay != sampleEndTime.TimeOfDay)
                                return false;

                        }

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

        private bool verifySameStartTime(ITeamBlockInfo teamBlockInfo, IEnumerable<DateOnly> dayList, IScheduleDay sampleScheduleDay)
        {
            var dateTimePeriod = getShiftPeriod(sampleScheduleDay.GetEditorShift());
            if (dateTimePeriod.HasValue)
            {
                var sampleStartTime = dateTimePeriod.Value.StartDateTimeLocal(sampleScheduleDay.TimeZone);
                foreach (var day in dayList)
                    foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(day))
                    {
                        var scheduleDay = matrix.GetScheduleDayByKey(day).DaySchedulePart();
                        if (isValidScheduleDay(scheduleDay))
                        {
                            var startDateTime = getStartTimeLocal(scheduleDay);
                            if (startDateTime != DateTime.MinValue && startDateTime.TimeOfDay != sampleStartTime.TimeOfDay)
                                return false;
                            
                        }

                    }
            }
            
            return true;
        }

        private DateTime getStartTimeLocal(IScheduleDay scheduleDay)
        {
            var dateTimePeriod = getShiftPeriod(scheduleDay.GetEditorShift());
            if (dateTimePeriod.HasValue)
            {
                return dateTimePeriod.Value.StartDateTimeLocal(scheduleDay.TimeZone);
            }
            return DateTime.MinValue ;
        }
        
        private DateTime getEndTimeLocal(IScheduleDay scheduleDay)
        {
            var dateTimePeriod = getShiftPeriod(scheduleDay.GetEditorShift());
            if (dateTimePeriod.HasValue)
            {
                return dateTimePeriod.Value.EndDateTimeLocal(scheduleDay.TimeZone);
            }
            return DateTime.MinValue ;
        }

        private static DateTimePeriod? getShiftPeriod(IEditableShift editableShift)
        {
			if (editableShift != null)
            {
				return editableShift.ProjectionService().CreateProjection().Period();
            }
            return null;
        }

        private static IShiftCategory getShiftCategory(IScheduleDay sampleScheduleDay)
        {
            var personAssignment = sampleScheduleDay.PersonAssignment();
            if (personAssignment != null && personAssignment.ShiftCategory != null)
                return personAssignment.ShiftCategory;    
            return null;
        }

        private bool verifySameShift(ITeamBlockInfo teamBlockInfo, IList<DateOnly> dayList, IScheduleDay sampleScheduleDay)
        {
            foreach (var day in dayList)
                foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(day))
                {
                    var scheduleDay = matrix.GetScheduleDayByKey(day).DaySchedulePart();
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