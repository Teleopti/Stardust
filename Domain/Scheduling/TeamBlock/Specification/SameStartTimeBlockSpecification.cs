using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification
{
    public interface ISameStartTimeBlockSpecification
    {
        bool IsSatisfiedBy(ITeamBlockInfo teamBlockInfo);
    }

    public class SameStartTimeBlockSpecification : Specification<ITeamBlockInfo>,
                                                       ISameStartTimeBlockSpecification
    {
        public override bool IsSatisfiedBy(ITeamBlockInfo teamBlockInfo)
        {
            var dayList = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
	        var matrixes = teamBlockInfo.TeamInfo.MatrixesForGroup();
            foreach (IScheduleMatrixPro matrix in matrixes)
            {
                IScheduleRange range = matrix.ActiveScheduleRange;
				TimeSpan sameStartInBlock = TimeSpan.MinValue;
                foreach (DateOnly dateOnly in dayList)
                {
	                IScheduleDay scheduleDay = range.ScheduledDay(dateOnly);
					if(scheduleDay == null) continue;
                    if (scheduleDay.SignificantPart() == SchedulePartView.MainShift)
                    {
                        DateTimePeriod? mainShiftPeriod =
                            scheduleDay.GetEditorShift().ProjectionService().CreateProjection().Period();
                        DateTime mainShiftStartTime = mainShiftPeriod.GetValueOrDefault().StartDateTime;
	                    if (sameStartInBlock.Equals(TimeSpan.MinValue))
		                    sameStartInBlock = mainShiftStartTime.TimeOfDay;
						if (mainShiftStartTime.TimeOfDay != sameStartInBlock)
                            return false;
                    }
                }
            }

            return true;
        }
    }
}