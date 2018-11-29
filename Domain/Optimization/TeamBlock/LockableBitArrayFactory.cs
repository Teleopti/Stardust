using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface ILockableBitArrayFactory
	{
		ILockableBitArray ConvertFromMatrix(bool useWeekBefore, bool useWeekAfter, IScheduleMatrixPro matrix);
	}

	public class LockableBitArrayFactory : ILockableBitArrayFactory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public ILockableBitArray ConvertFromMatrix(bool useWeekBefore, bool useWeekAfter, IScheduleMatrixPro matrix)
		{
			if (!useWeekBefore && !useWeekAfter)
				return arrayFromList(matrix.FullWeeksPeriodDays, false, false, matrix);

			if (useWeekBefore && !useWeekAfter)
				return arrayFromList(matrix.WeekBeforeOuterPeriodDays, true, false, matrix);

			if (!useWeekBefore)
				return arrayFromList(matrix.WeekAfterOuterPeriodDays, false, true, matrix);

			return arrayFromList(matrix.OuterWeeksPeriodDays, true, true, matrix);
		}

		private static ILockableBitArray arrayFromList(IList<IScheduleDayPro> list, bool useWeekBefore, bool useWeekAfter, IScheduleMatrixPro matrix)
		{
			ILockableBitArray ret = new LockableBitArray(list.Count, useWeekBefore, useWeekAfter, null);
			int index = 0;
			foreach (IScheduleDayPro scheduleDayPro in list)
			{
				SchedulePartView significant = scheduleDayPro.DaySchedulePart().SignificantPart();
				ret.Set(index, (significant == SchedulePartView.DayOff) || (significant == SchedulePartView.ContractDayOff));
				if (!matrix.UnlockedDays.Contains(scheduleDayPro))
					ret.Lock(index, true);

				index++;
			}

			var matrixEffectivePeriodDays = matrix.EffectivePeriodDays;
			int periodAreaStart = list.IndexOf(matrixEffectivePeriodDays[0]);
			int periodAreaEnd = list.IndexOf(matrixEffectivePeriodDays[matrixEffectivePeriodDays.Length - 1]);
			ret.PeriodArea = new MinMax<int>(periodAreaStart, periodAreaEnd);

			return ret;
		}
	}
}