using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification
{
	public interface ISameStartTimeTeamSpecification
    {
        bool IsSatisfiedBy(ITeamBlockInfo teamBlockInfo);
    }

	public class SameStartTimeTeamSpecification : Specification<ITeamBlockInfo>,
                                                       ISameStartTimeTeamSpecification
    {
		public override bool IsSatisfiedBy(ITeamBlockInfo teamBlockInfo)
		{
			IList<DateOnly> dayList = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
			List<IScheduleMatrixPro> matrixes = teamBlockInfo.TeamInfo.MatrixesForGroup().ToList();
			foreach (DateOnly dateOnly in dayList)
			{
				TimeSpan sameStartInBlock = TimeSpan.MinValue;

				foreach (IScheduleMatrixPro matrix in matrixes)
				{
					IScheduleRange range = matrix.ActiveScheduleRange;

					IScheduleDay scheduleDay = range.ScheduledDay(dateOnly);
					if (scheduleDay == null) continue;
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