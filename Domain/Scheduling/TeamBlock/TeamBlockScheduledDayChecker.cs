using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public static class TeamBlockScheduledDayChecker
    {
        public static bool IsDayScheduledInTeamBlock(ITeamBlockInfo teamBlockInfo, DateOnly dateOnly)
        {
            if (teamBlockInfo == null) return false;
            IScheduleRange rangeForPerson = null;
            foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroup())
            {
                rangeForPerson = matrix.SchedulingStateHolder.Schedules[matrix.Person];
                break;
            }
            if (rangeForPerson == null) return false;

            IScheduleDay scheduleDay = rangeForPerson.ScheduledDay(dateOnly);
            if (!scheduleDay.IsScheduled())
                return false;
            
            return true;
        }
    }
}