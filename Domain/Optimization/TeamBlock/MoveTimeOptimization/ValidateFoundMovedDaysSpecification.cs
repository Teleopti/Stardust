using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization
{
    public interface IValidateFoundMovedDaysSpecification
    {
        bool AreFoundDaysValid(int currentMoveFromIndex, int currentMoveToIndex, IScheduleMatrixPro matrix);
    }
    public class ValidateFoundMovedDaysSpecification : IValidateFoundMovedDaysSpecification
    {
        public bool AreFoundDaysValid(int currentMoveFromIndex, int currentMoveToIndex, IScheduleMatrixPro matrix)
        {
            IScheduleDayPro currentMoveFromDay = matrix.FullWeeksPeriodDays[currentMoveFromIndex];
            IScheduleDayPro currentMoveToDay = matrix.FullWeeksPeriodDays[currentMoveToIndex];
            var currentMoveFromDaySchedule = currentMoveFromDay.DaySchedulePart();
            var currentMoveToDaySchedule = currentMoveToDay.DaySchedulePart();

            if (currentMoveFromDaySchedule.SignificantPart() != SchedulePartView.MainShift
                || currentMoveToDaySchedule.SignificantPart() != SchedulePartView.MainShift)
                return false;

            TimeSpan? moveFromWorkShiftLength = null;
            TimeSpan? moveToWorkShiftLength = null;


            if (currentMoveFromDaySchedule != null 
                && currentMoveFromDaySchedule.ProjectionService() != null)
                moveFromWorkShiftLength = currentMoveFromDaySchedule.ProjectionService().CreateProjection().ContractTime();
            if (currentMoveToDaySchedule != null 
                && currentMoveToDaySchedule.ProjectionService() != null)
                moveToWorkShiftLength = currentMoveToDaySchedule.ProjectionService().CreateProjection().ContractTime();
            if (moveFromWorkShiftLength.HasValue
                && moveToWorkShiftLength.HasValue
                && moveFromWorkShiftLength.Value > moveToWorkShiftLength.Value)
                return false;

            return true;
        }
    }
}