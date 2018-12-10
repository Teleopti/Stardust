using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	//Could probably be removed/refactored later. Especially when toggle 37049 is gone. "Find most important day" could be based on all group instead
	//For now - just make tests go green....
	public interface IScheduleMatrixLockableBitArrayConverterEx
	{
		ILockableBitArray Convert(IScheduleMatrixPro matrix, bool useWeekBefore, bool useWeekAfter, IEnumerable<DateOnly> skipDates);
		ILockableBitArray Convert(IScheduleMatrixPro matrix, bool useWeekBefore, bool useWeekAfter);
	}

	public class ScheduleMatrixLockableBitArrayConverterEx : IScheduleMatrixLockableBitArrayConverterEx
	{
		public ILockableBitArray Convert(IScheduleMatrixPro matrix, bool useWeekBefore, bool useWeekAfter)
		{
			return Convert(matrix, useWeekBefore, useWeekAfter, Enumerable.Empty<DateOnly>());
		}

		public ILockableBitArray Convert(IScheduleMatrixPro matrix, bool useWeekBefore, bool useWeekAfter, IEnumerable<DateOnly> skipDates)
		{
			if (!useWeekBefore && !useWeekAfter)
				return arrayFromList(matrix, matrix.FullWeeksPeriodDays, false, false, skipDates);

			if (useWeekBefore && !useWeekAfter)
				return arrayFromList(matrix, matrix.WeekBeforeOuterPeriodDays, true, false, skipDates);

			if (!useWeekBefore)
				return arrayFromList(matrix, matrix.WeekAfterOuterPeriodDays, false, true, skipDates);

			return arrayFromList(matrix, matrix.OuterWeeksPeriodDays, true, true, skipDates);
		}

		private static ILockableBitArray arrayFromList(IScheduleMatrixPro matrix, IList<IScheduleDayPro> list, bool useWeekBefore, bool useWeekAfter, IEnumerable<DateOnly> dontUseOnDates)
		{
			ILockableBitArray ret = new LockableBitArray(list.Count, useWeekBefore, useWeekAfter, null);
			int index = 0;
			foreach (IScheduleDayPro scheduleDayPro in list)
			{
				SchedulePartView significant = scheduleDayPro.DaySchedulePart().SignificantPart();
				ret.Set(index, ((significant == SchedulePartView.DayOff) || (significant == SchedulePartView.ContractDayOff)));
				if (!matrix.UnlockedDays.Contains(scheduleDayPro))
					ret.Lock(index, true);
				if (!scheduleDayPro.DaySchedulePart().PersonAbsenceCollection().IsEmpty())
					ret.Lock(index, true);
				if (dontUseOnDates.Contains(scheduleDayPro.Day))
					ret.Lock(index, true);
				index++;
			}

			int periodAreaStart = list.IndexOf(matrix.EffectivePeriodDays[0]);
			int periodAreaEnd = list.IndexOf(matrix.EffectivePeriodDays[matrix.EffectivePeriodDays.Length - 1]);
			ret.PeriodArea = new MinMax<int>(periodAreaStart, periodAreaEnd);

			return ret;
		}
	}
}