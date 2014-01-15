using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface IValidSampleDayPickerFromTeamBlock
    {
        IScheduleDay GetSampleScheduleDay(ITeamBlockInfo teamBlockInfo );
    }

    public class ValidSampleDayPickerFromTeamBlock : IValidSampleDayPickerFromTeamBlock
    {
        public IScheduleDay GetSampleScheduleDay(ITeamBlockInfo teamBlockInfo )
        {
            var dayList = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
            var matrixes = teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(dayList[0]).ToList();
            foreach (var matrix in matrixes)
            {
                var range = matrix.ActiveScheduleRange;
                foreach (var dateOnly in dayList)
                {
                    var scheduleDay = range.ScheduledDay(dateOnly);
                    if (scheduleDay.SignificantPart() == SchedulePartView.MainShift)
                        return scheduleDay;
                }
            }

            return null;
        }
    }
}
