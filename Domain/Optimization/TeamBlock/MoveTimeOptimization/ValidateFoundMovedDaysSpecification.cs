using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization
{
    public class ValidateFoundMovedDaysSpecification
    {
        public bool AreFoundDaysValid(int currentMoveFromIndex, int currentMoveToIndex, IScheduleMatrixPro matrix)
        {
            IScheduleDayPro currentMoveFromDay = matrix.FullWeeksPeriodDays[currentMoveFromIndex];
            IScheduleDayPro currentMoveToDay = matrix.FullWeeksPeriodDays[currentMoveToIndex];

            if (currentMoveFromDay.DaySchedulePart().SignificantPart() != SchedulePartView.MainShift
                || currentMoveToDay.DaySchedulePart().SignificantPart() != SchedulePartView.MainShift)
                return false;


            TimeSpan? moveFromWorkShiftLength = null;
            TimeSpan? moveToWorkShiftLength = null;

            if (currentMoveFromDay.DaySchedulePart() != null
                && currentMoveFromDay.DaySchedulePart().ProjectionService() != null)
                moveFromWorkShiftLength = currentMoveFromDay.DaySchedulePart().ProjectionService().CreateProjection().ContractTime();
            if (currentMoveToDay.DaySchedulePart() != null
                && currentMoveToDay.DaySchedulePart().ProjectionService() != null)
                moveToWorkShiftLength = currentMoveToDay.DaySchedulePart().ProjectionService().CreateProjection().ContractTime();
            if (moveFromWorkShiftLength.HasValue
                && moveToWorkShiftLength.HasValue
                && moveFromWorkShiftLength.Value > moveToWorkShiftLength.Value)
                return false;

            return true;
        }
    }
}