using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
    public interface IWorkShiftBackToLegalStateBitArrayCreator
    {
        ILockableBitArray CreateWeeklyBitArray(int weekIndex, IScheduleMatrixPro scheduleMatrix);
        ILockableBitArray CreatePeriodBitArray(IScheduleMatrixPro scheduleMatrix);
    }

    public class WorkShiftBackToLegalStateBitArrayCreator : IWorkShiftBackToLegalStateBitArrayCreator
    {

        public ILockableBitArray CreateWeeklyBitArray(int weekIndex, IScheduleMatrixPro scheduleMatrix)
        {
            IList<IScheduleDayPro> daysList = scheduleMatrix.FullWeeksPeriodDays;
            ILockableBitArray ret = new LockableBitArray(daysList.Count, false, false, null);
            var validWeekIndexes = new MinMax<int>(weekIndex * 7, weekIndex * 7 + 6);
            int index = -1;
            foreach (IScheduleDayPro scheduleDayPro in daysList)
            {
                index++;

                SchedulePartView significantPart = scheduleDayPro.DaySchedulePart().SignificantPart();
                if (!scheduleMatrix.UnlockedDays.Contains(scheduleDayPro))
                {
                    ret.Lock(index, true);
                    continue;
                }
                if (significantPart != SchedulePartView.MainShift)
                {
                    ret.Lock(index, true);
                    continue;
                }
                if (index < validWeekIndexes.Minimum || index > validWeekIndexes.Maximum) 
                {
                    ret.Lock(index, true);
                }
            }

            int periodAreaStart = daysList.IndexOf(scheduleMatrix.EffectivePeriodDays[0]);
            int periodAreaEnd = daysList.IndexOf(scheduleMatrix.EffectivePeriodDays[scheduleMatrix.EffectivePeriodDays.Length - 1]);
            ret.PeriodArea = new MinMax<int>(periodAreaStart, periodAreaEnd);

            return ret;
        }

        public ILockableBitArray CreatePeriodBitArray(IScheduleMatrixPro scheduleMatrix)
        {
            IList<IScheduleDayPro> daysList = scheduleMatrix.FullWeeksPeriodDays;
            ILockableBitArray ret = new LockableBitArray(daysList.Count, false, false, null);
            int index = -1;
            foreach (IScheduleDayPro scheduleDayPro in daysList)
            {
                index++;

                SchedulePartView significantPart = scheduleDayPro.DaySchedulePart().SignificantPart();
                if (!scheduleMatrix.UnlockedDays.Contains(scheduleDayPro))
                {
                    ret.Lock(index, true);
                    continue;
                }
                if (significantPart != SchedulePartView.MainShift)
                {
                    ret.Lock(index, true);
                }
            }

            int periodAreaStart = daysList.IndexOf(scheduleMatrix.EffectivePeriodDays[0]);
            int periodAreaEnd = daysList.IndexOf(scheduleMatrix.EffectivePeriodDays[scheduleMatrix.EffectivePeriodDays.Length - 1]);
            ret.PeriodArea = new MinMax<int>(periodAreaStart, periodAreaEnd);

            return ret;
        }
    }
}
