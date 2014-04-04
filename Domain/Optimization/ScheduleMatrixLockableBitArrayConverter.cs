using System.Collections.Generic;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class ScheduleMatrixLockableBitArrayConverter : IScheduleMatrixLockableBitArrayConverter
    {
        private readonly IScheduleMatrixPro _matrix;

        public ScheduleMatrixLockableBitArrayConverter(IScheduleMatrixPro matrix)
        {
            _matrix = matrix;
        }

        public ILockableBitArray Convert(bool useWeekBefore, bool useWeekAfter)
        {
            //int? terminalDateIndex = _matrix.Person.TerminalDate

            if(!useWeekBefore && !useWeekAfter)
                return arrayFromList(_matrix.FullWeeksPeriodDays, false, false);

            if (useWeekBefore && !useWeekAfter)
                return arrayFromList(_matrix.WeekBeforeOuterPeriodDays, true, false);

            if (!useWeekBefore)
                return arrayFromList(_matrix.WeekAfterOuterPeriodDays, false, true);

            return arrayFromList(_matrix.OuterWeeksPeriodDays, true, true);
        }

        public IScheduleMatrixPro SourceMatrix
        {
            get { return _matrix; }
        }

        public int Workdays()
        {
            int workdayCount = 0;
            foreach (IScheduleDayPro scheduleDayPro in _matrix.EffectivePeriodDays)
            {
                if (scheduleDayPro.DaySchedulePart().SignificantPart() == SchedulePartView.MainShift)
                    workdayCount++;
            }
            return workdayCount;
        }

        public int DayOffs()
        {
            int count = 0;
            foreach (IScheduleDayPro scheduleDayPro in _matrix.EffectivePeriodDays)
            {
                SchedulePartView significant = scheduleDayPro.DaySchedulePart().SignificantPart();
                if(significant == SchedulePartView.DayOff || significant == SchedulePartView.ContractDayOff)
                    count++;
            }
            return count;
        }

        private ILockableBitArray arrayFromList(IList<IScheduleDayPro> list, bool useWeekBefore, bool useWeekAfter)
        {
            ILockableBitArray ret = new LockableBitArray(list.Count, useWeekBefore, useWeekAfter, null);
            int index = 0;
            foreach (IScheduleDayPro scheduleDayPro in list)
            {
            	SchedulePartView significant = scheduleDayPro.DaySchedulePart().SignificantPart();
                ret.Set(index, ((significant == SchedulePartView.DayOff) || (significant == SchedulePartView.ContractDayOff)));
                if(!_matrix.UnlockedDays.Contains(scheduleDayPro))
                    ret.Lock(index, true);
                if (scheduleDayPro.DaySchedulePart().SignificantPart() == SchedulePartView.FullDayAbsence)
                    ret.Lock(index, true);
                index++;
            }

            int periodAreaStart = list.IndexOf(_matrix.EffectivePeriodDays[0]);
            int periodAreaEnd = list.IndexOf(_matrix.EffectivePeriodDays[_matrix.EffectivePeriodDays.Count - 1]);
            ret.PeriodArea = new MinMax<int>(periodAreaStart, periodAreaEnd);

            return ret;
        }
        
    }
}