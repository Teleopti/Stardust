using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface ITeamBlockSameStartTimeSpecification
    {
        bool IsSatisfiedBy(ITeamBlockInfo teamBlockInfo);
    }

    public class TeamBlockSameStartTimeSpecification : Specification<ITeamBlockInfo>,
                                                       ITeamBlockSameStartTimeSpecification
    {
        private readonly IValidSampleDayPickerFromTeamBlock _validSampleDayPickerFromTeamBlock;

        public TeamBlockSameStartTimeSpecification(IValidSampleDayPickerFromTeamBlock validSampleDayPickerFromTeamBlock)
        {
            _validSampleDayPickerFromTeamBlock = validSampleDayPickerFromTeamBlock;
        }

        public override bool IsSatisfiedBy(ITeamBlockInfo teamBlockInfo)
        {
            IScheduleDay sampleScheduleDay = _validSampleDayPickerFromTeamBlock.GetSampleScheduleDay(teamBlockInfo);
            if (sampleScheduleDay == null)
                return true;
            IList<DateOnly> dayList = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
            List<IScheduleMatrixPro> matrixes = teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(dayList[0]).ToList();
            IEditableShift sampleShift = sampleScheduleDay.GetEditorShift();
            DateTimePeriod sampleMainShiftPeriod =
                sampleShift.ProjectionService().CreateProjection().Period().GetValueOrDefault();
            DateTime sampleStartTime = sampleMainShiftPeriod.StartDateTime;
            foreach (IScheduleMatrixPro matrix in matrixes)
            {
                IScheduleRange range = matrix.ActiveScheduleRange;
                foreach (DateOnly dateOnly in dayList)
                {
                    IScheduleDay scheduleDay = range.ScheduledDay(dateOnly);
                    if (scheduleDay.SignificantPart() == SchedulePartView.MainShift)
                    {
                        DateTimePeriod? mainShiftPeriod =
                            scheduleDay.GetEditorShift().ProjectionService().CreateProjection().Period();
                        DateTime mainShiftStartTime = mainShiftPeriod.GetValueOrDefault().StartDateTime;
                        if (mainShiftStartTime.TimeOfDay != sampleStartTime.TimeOfDay)
                            return false;
                    }
                }
            }

            return true;
        }
    }
}