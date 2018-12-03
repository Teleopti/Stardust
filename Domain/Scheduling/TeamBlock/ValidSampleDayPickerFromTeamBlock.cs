using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface IValidSampleDayPickerFromTeamBlock
    {
        IScheduleDay GetSampleScheduleDay(ITeamBlockInfo teamBlockInfo);
	    IScheduleDay GetSampleScheduleDay(ITeamBlockInfo teamBlockInfo, IPerson person);
    }

    public class ValidSampleDayPickerFromTeamBlock : IValidSampleDayPickerFromTeamBlock
    {
        public IScheduleDay GetSampleScheduleDay(ITeamBlockInfo teamBlockInfo)
        {
            var dayList = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
            var matrixes = teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(dayList[0]).ToList();
            foreach (var matrix in matrixes)
            {
	            var sample = sampleScheduleDay(matrix, dayList);
	            if (sample != null)
		            return sample;
            }

	        return null;
        }

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
