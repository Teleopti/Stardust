using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class ShiftCategoryLimitCounter
	{
		public bool HaveMaxOfShiftCategory(IShiftCategoryLimitation shiftCategoryLimitation, ITeamInfo teamInfo, DateOnly dateOnly)
		{
			if (shiftCategoryLimitation.Weekly) return haveMaxOfShiftCategoryOnWeek(shiftCategoryLimitation, teamInfo, dateOnly);
			return haveMaxOfShiftCategoryOnPeriod(shiftCategoryLimitation, teamInfo);
		}

		private bool haveMaxOfShiftCategoryOnWeek(IShiftCategoryLimitation shiftCategoryLimitation, ITeamInfo teamInfo, DateOnly dateOnly)
		{
			foreach (var scheduleMatrixPro in teamInfo.MatrixesForGroupAndDate(dateOnly))
			{
				IList<IScheduleDayPro> days = scheduleMatrixPro.FullWeeksPeriodDays;

				for (var week = 0; week < days.Count; week += 7)
				{
					var dateOnlyDay = days[week].Day;
					var periodForWeek = new DateOnlyPeriod(dateOnlyDay, dateOnlyDay.AddDays(6));
					if (!periodForWeek.Contains(dateOnly)) continue;
					var counter = 0;
					for (var day = 0; day < 7; day++)
					{
						var part = days[week + day].DaySchedulePart();
						if (part.SignificantPart() == SchedulePartView.MainShift && part.PersonAssignment().ShiftCategory.Equals(shiftCategoryLimitation.ShiftCategory)) counter++;
					}

					if (counter >= shiftCategoryLimitation.MaxNumberOf) return true;
				}
			}

			return false;	
		}

		private bool haveMaxOfShiftCategoryOnPeriod(IShiftCategoryLimitation shiftCategoryLimitation, ITeamInfo teamInfo)
		{
			foreach (var scheduleMatrixPro in teamInfo.MatrixesForGroup())
			{
				var counter = 0;

				foreach (var scheduleDayPro in scheduleMatrixPro.EffectivePeriodDays)
				{
					var part = scheduleDayPro.DaySchedulePart();
					if (part.SignificantPart() == SchedulePartView.MainShift && part.PersonAssignment().ShiftCategory.Equals(shiftCategoryLimitation.ShiftCategory)) counter++;
				}

				if (counter >= shiftCategoryLimitation.MaxNumberOf) return true;
			}

			return false;
		}
	}
}
