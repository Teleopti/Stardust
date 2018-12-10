using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface IValidSampleDayPickerFromTeamBlock
    {
        IScheduleDay GetSampleScheduleDay(ITeamBlockInfo teamBlockInfo, IPerson person);
    }

    public class ValidSampleDayPickerFromTeamBlock : IValidSampleDayPickerFromTeamBlock
    {
	    private static IScheduleDay sampleScheduleDay(IScheduleMatrixPro matrix, IList<DateOnly> dayList)
	    {
		    var range = matrix.ActiveScheduleRange;
		    foreach (var dateOnly in dayList)
		    {
			    var scheduleDay = range.ScheduledDay(dateOnly);
			    if (scheduleDay.SignificantPart() == SchedulePartView.MainShift)
				    return scheduleDay;
		    }

			return null;
	    }

	    public IScheduleDay GetSampleScheduleDay(ITeamBlockInfo teamBlockInfo, IPerson person)
	    {
		    var dayList = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
		    var matrix = teamBlockInfo.TeamInfo.MatrixForMemberAndDate(person, dayList[0]);

		    return sampleScheduleDay(matrix, dayList);
	    }
    }
}
