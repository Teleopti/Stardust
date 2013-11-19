using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification
{
    public interface ISameShiftBlockSpecification
    {
        bool IsSatisfiedBy(ITeamBlockInfo teamBlockInfo);
    }

    public class SameShiftBlockSpecification : Specification<ITeamBlockInfo>,
                                                       ISameShiftBlockSpecification
    {
        private readonly IValidSampleDayPickerFromTeamBlock _validSampleDayPickerFromTeamBlock;
        private readonly IScheduleDayEquator _scheduleDayEquator;

		public SameShiftBlockSpecification(IValidSampleDayPickerFromTeamBlock validSampleDayPickerFromTeamBlock, IScheduleDayEquator scheduleDayEquator)
        {
            _validSampleDayPickerFromTeamBlock = validSampleDayPickerFromTeamBlock;
            _scheduleDayEquator = scheduleDayEquator;
        }

        public override bool IsSatisfiedBy(ITeamBlockInfo teamBlockInfo)
        {
            IScheduleDay sampleScheduleDay = _validSampleDayPickerFromTeamBlock.GetSampleScheduleDay(teamBlockInfo);
            if (sampleScheduleDay == null)
                return true;
            IList<DateOnly> dayList = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
            List<IScheduleMatrixPro> matrixes = teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(dayList[0]).ToList();
            var sampleShift = sampleScheduleDay.GetEditorShift();
            foreach (var matrix in matrixes)
            {
                var range = matrix.ActiveScheduleRange;
                foreach (var dateOnly in dayList)
                {
                    var scheduleDay = range.ScheduledDay(dateOnly);
                    if (scheduleDay.SignificantPart() == SchedulePartView.MainShift)
                    {
                        var destShift = scheduleDay.GetEditorShift();
                        if ((!_scheduleDayEquator.MainShiftEquals(sampleShift, destShift)))
                            return false;
                    }
                }
            }

            return true;
        }
    }
}