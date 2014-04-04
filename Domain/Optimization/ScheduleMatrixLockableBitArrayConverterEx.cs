

using System.Collections.Generic;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IScheduleMatrixLockableBitArrayConverterEx
	{
		ILockableBitArray Convert(IScheduleMatrixPro matrix, bool useWeekBefore, bool useWeekAfter);
	}

	public class ScheduleMatrixLockableBitArrayConverterEx : IScheduleMatrixLockableBitArrayConverterEx
	{
		public ILockableBitArray Convert(IScheduleMatrixPro matrix, bool useWeekBefore, bool useWeekAfter)
		{
			if (!useWeekBefore && !useWeekAfter)
				return arrayFromList(matrix, matrix.FullWeeksPeriodDays, false, false);

			if (useWeekBefore && !useWeekAfter)
				return arrayFromList(matrix, matrix.WeekBeforeOuterPeriodDays, true, false);

			if (!useWeekBefore)
				return arrayFromList(matrix, matrix.WeekAfterOuterPeriodDays, false, true);

			return arrayFromList(matrix, matrix.OuterWeeksPeriodDays, true, true);
		}

		private ILockableBitArray arrayFromList(IScheduleMatrixPro matrix, IList<IScheduleDayPro> list, bool useWeekBefore, bool useWeekAfter)
		{
			ILockableBitArray ret = new LockableBitArray(list.Count, useWeekBefore, useWeekAfter, null);
			int index = 0;
			foreach (IScheduleDayPro scheduleDayPro in list)
			{
				SchedulePartView significant = scheduleDayPro.DaySchedulePart().SignificantPart();
				ret.Set(index, ((significant == SchedulePartView.DayOff) || (significant == SchedulePartView.ContractDayOff)));
				if (!matrix.UnlockedDays.Contains(scheduleDayPro))
					ret.Lock(index, true);
				if (scheduleDayPro.DaySchedulePart().SignificantPart() == SchedulePartView.FullDayAbsence)
					ret.Lock(index, true);
				index++;
			}

			int periodAreaStart = list.IndexOf(matrix.EffectivePeriodDays[0]);
			int periodAreaEnd = list.IndexOf(matrix.EffectivePeriodDays[matrix.EffectivePeriodDays.Count - 1]);
			ret.PeriodArea = new MinMax<int>(periodAreaStart, periodAreaEnd);

			return ret;
		}
	}
}