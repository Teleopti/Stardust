using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
    public interface IScheduleDayWorkShiftTimeExtractor
    {
        DateTimePeriod? ShiftStartEndTime(IScheduleDay scheduleDay);
    }

    public class ScheduleDayWorkShiftTimeExtractor : IScheduleDayWorkShiftTimeExtractor
    {
        private readonly IWorkTimeStartEndExtractor _workTimeStartEndExtractor;

        public ScheduleDayWorkShiftTimeExtractor(IWorkTimeStartEndExtractor workTimeStartEndExtractor)
        {
            _workTimeStartEndExtractor = workTimeStartEndExtractor;
        }

        public DateTimePeriod? ShiftStartEndTime(IScheduleDay scheduleDay)
        {
            if (scheduleDay.SignificantPart() == SchedulePartView.MainShift)
            {
                var proj = scheduleDay.PersonAssignment().ProjectionService().CreateProjection();
                var startTime = _workTimeStartEndExtractor.WorkTimeStart(proj);
                var endTime = _workTimeStartEndExtractor.WorkTimeEnd(proj);
                if (startTime.HasValue && endTime.HasValue)
                    return new DateTimePeriod(startTime.Value, endTime.Value);
            }
            return null;
        }
    }
}