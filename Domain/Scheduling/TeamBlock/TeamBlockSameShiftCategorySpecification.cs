using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface ITeamBlockSameShiftCategorySpecification
    {
        bool IsSatisfiedBy(ITeamBlockInfo teamBlockInfo );
    }

    public class TeamBlockSameShiftCategorySpecification : Specification<ITeamBlockInfo>, ITeamBlockSameShiftCategorySpecification
    {
        private readonly IValidSampleDayPickerFromTeamBlock _validSampleDayPickerFromTeamBlock;

        public TeamBlockSameShiftCategorySpecification(IValidSampleDayPickerFromTeamBlock validSampleDayPickerFromTeamBlock )
        {
            _validSampleDayPickerFromTeamBlock = validSampleDayPickerFromTeamBlock;
        }

        public override bool IsSatisfiedBy(ITeamBlockInfo teamBlockInfo)
        {
            var sampleScheduleDay = _validSampleDayPickerFromTeamBlock.GetSampleScheduleDay(teamBlockInfo);
            if (sampleScheduleDay == null)
                return true;
            var dayList = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
            var matrixes = teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(dayList[0]).ToList();
            var sampleShiftCategory = sampleScheduleDay.PersonAssignment().ShiftCategory;
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


    }

    
}
