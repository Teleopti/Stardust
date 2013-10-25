using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{

    public interface ITeamBlockSteadyStateValidator
    {
        bool IsBlockInSteadyState(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions);
    }

    public class TeamBlockSteadyStateValidator : ITeamBlockSteadyStateValidator
    {
	    private readonly IScheduleDayEquator _scheduleDayEquator;

	    public TeamBlockSteadyStateValidator(IScheduleDayEquator scheduleDayEquator)
		{
			_scheduleDayEquator = scheduleDayEquator;
		}

	    public bool IsBlockInSteadyState(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions)
	    {
		    var dayList = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
		    var matrixList = teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(dayList[0]).ToList();
		    var sampleScheduleDay = getValidSampleDay(matrixList, dayList);
		    if (sampleScheduleDay == null)
			    return true;

		    var sampleShift = sampleScheduleDay.GetEditorShift();
			var sampleMainShiftPeriod = sampleShift.ProjectionService().CreateProjection().Period().GetValueOrDefault();
		    if (schedulingOptions.UseTeamBlockSameStartTime)
				return verifySameStartTime(matrixList, dayList, sampleMainShiftPeriod);
			if (schedulingOptions.UseTeamBlockSameEndTime)
				return verifySameEndTime(matrixList, dayList, sampleMainShiftPeriod);
		    if (schedulingOptions.UseTeamBlockSameShift)
				return verifySameShift(matrixList, dayList, sampleShift);
		    if (schedulingOptions.UseTeamBlockSameShiftCategory)
				return verifySameShiftCategory(matrixList, dayList, sampleScheduleDay.PersonAssignment().ShiftCategory);

		    return true;
	    }

	    private IScheduleDay getValidSampleDay(IEnumerable<IScheduleMatrixPro> matrixes, IList<DateOnly> dayList)
        {
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

	    private bool verifySameShiftCategory(IEnumerable<IScheduleMatrixPro> matrixes, IList<DateOnly> dayList,
	                                         IShiftCategory sampleShiftCategory)
	    {
		    foreach (var matrix in matrixes)
		    {
			    var range = matrix.ActiveScheduleRange;
			    foreach (var dateOnly in dayList)
			    {
				    var scheduleDay = range.ScheduledDay(dateOnly);
				    if (scheduleDay.SignificantPart() == SchedulePartView.MainShift)
				    {
					    if (scheduleDay.PersonAssignment().ShiftCategory != sampleShiftCategory)
						    return false;
				    }
			    }
		    }

		    return true;
	    }

	    private bool verifySameEndTime(IEnumerable<IScheduleMatrixPro> matrixes, IList<DateOnly> dayList,
	                                   DateTimePeriod sampleMainShiftPeriod)
	    {
		    var sampleEndTime = sampleMainShiftPeriod.EndDateTime;
		    foreach (var matrix in matrixes)
		    {
			    var range = matrix.ActiveScheduleRange;
			    foreach (var dateOnly in dayList)
			    {
				    var scheduleDay = range.ScheduledDay(dateOnly);
				    if (scheduleDay.SignificantPart() == SchedulePartView.MainShift)
				    {
					    var mainShiftPeriod = scheduleDay.GetEditorShift().ProjectionService().CreateProjection().Period();
					    var mainShiftEndTime = mainShiftPeriod.GetValueOrDefault().EndDateTime;
					    if (mainShiftEndTime.TimeOfDay  != sampleEndTime.TimeOfDay )
						    return false;
				    }
			    }
		    }

		    return true;
	    }

	    private bool verifySameStartTime(IEnumerable<IScheduleMatrixPro> matrixes, IList<DateOnly> dayList,
	                                     DateTimePeriod sampleMainShiftPeriod)
	    {
		    var sampleStartTime = sampleMainShiftPeriod.StartDateTime;
		    foreach (var matrix in matrixes)
		    {
			    var range = matrix.ActiveScheduleRange;
			    foreach (var dateOnly in dayList)
			    {
				    var scheduleDay = range.ScheduledDay(dateOnly);
				    if (scheduleDay.SignificantPart() == SchedulePartView.MainShift)
				    {
					    var mainShiftPeriod = scheduleDay.GetEditorShift().ProjectionService().CreateProjection().Period();
					    var mainShiftStartTime = mainShiftPeriod.GetValueOrDefault().StartDateTime;
					    if (mainShiftStartTime.TimeOfDay != sampleStartTime.TimeOfDay )
						    return false;
				    }
			    }
		    }

		    return true;
	    }

	    private bool verifySameShift(IEnumerable<IScheduleMatrixPro> matrixes, IList<DateOnly> dayList,
									 IEditableShift sourceShift)
	    {
		    foreach (var matrix in matrixes)
		    {
			    var range = matrix.ActiveScheduleRange;
			    foreach (var dateOnly in dayList)
			    {
				    var scheduleDay = range.ScheduledDay(dateOnly);
				    if (scheduleDay.SignificantPart() == SchedulePartView.MainShift)
				    {
					    var destShift = scheduleDay.GetEditorShift();
						if ((!_scheduleDayEquator.MainShiftEquals(sourceShift, destShift)))
						    return false;
				    }
			    }
		    }

		    return true;
	    }
    }
}