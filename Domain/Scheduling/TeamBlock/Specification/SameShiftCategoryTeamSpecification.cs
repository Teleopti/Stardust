using System.Linq;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification
{
	public interface ISameShiftCategoryTeamSpecification
	{
		bool IsSatisfiedBy(ITeamBlockInfo teamBlockInfo);
	}
	public class SameShiftCategoryTeamSpecification : Specification<ITeamBlockInfo>, ISameShiftCategoryTeamSpecification
	{
		public override bool IsSatisfiedBy(ITeamBlockInfo teamBlockInfo)
		{
			var dayList = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
			var matrixes = teamBlockInfo.TeamInfo.MatrixesForGroup().ToList();
			foreach (var dateOnly in dayList)
			{
				IShiftCategory sameShiftCategory = null;
				foreach (var matrix in matrixes)
				{
					var range = matrix.ActiveScheduleRange;
					var scheduleDay = range.ScheduledDay(dateOnly);
					if (scheduleDay == null) continue;
					if (scheduleDay.SignificantPart() == SchedulePartView.MainShift)
					{
						var shiftCategory = scheduleDay.PersonAssignment().ShiftCategory;
						if (sameShiftCategory == null)
							sameShiftCategory = shiftCategory;
						if (shiftCategory != sameShiftCategory)
							return false;
					}
				}
			}
			return true;
		}
	}

	
}
