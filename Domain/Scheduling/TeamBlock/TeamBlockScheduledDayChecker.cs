using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public static class TeamBlockScheduledDayChecker
    {
        public static bool IsDayScheduledInTeamBlock(ITeamBlockInfo teamBlockInfo, DateOnly dateOnly)
        {
            if (teamBlockInfo == null) return false;
           
            foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(dateOnly))
            {
                IScheduleRange rangeForPerson =  matrix.SchedulingStateHolder.Schedules[matrix.Person];
                IScheduleDay scheduleDay = rangeForPerson.ScheduledDay(dateOnly);
                if (!scheduleDay.IsScheduled())
                {
                    return false;
                }
            }

            return true;
            
        }
    }
}