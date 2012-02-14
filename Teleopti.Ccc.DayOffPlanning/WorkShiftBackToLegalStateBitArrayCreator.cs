﻿using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanning
{
    public interface IWorkShiftBackToLegalStateBitArrayCreator
    {
        ILockableBitArray CreateWeeklyBitArray(int weekIndex, IScheduleMatrixPro scheduleMatrix);
        ILockableBitArray CreatePeriodBitArray(bool raise, IScheduleMatrixPro scheduleMatrix);
    }

    public class WorkShiftBackToLegalStateBitArrayCreator : IWorkShiftBackToLegalStateBitArrayCreator
    {
       private readonly IPossibleMinMaxWorkShiftLengthExtractor _workShiftMinMaxCalculator;

        // for each weeks and period
        public WorkShiftBackToLegalStateBitArrayCreator(
            IPossibleMinMaxWorkShiftLengthExtractor workShiftMinMaxCalculator)
        {
            _workShiftMinMaxCalculator = workShiftMinMaxCalculator;
        }

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
                    continue;
                }
                MinMax<TimeSpan> minMaxLength = _workShiftMinMaxCalculator.PossibleLengthsForDate(scheduleDayPro.Day, scheduleMatrix);
                TimeSpan workingTime = scheduleDayPro.DaySchedulePart().ProjectionService().CreateProjection().ContractTime();
                if(workingTime <= minMaxLength.Minimum)
                {
                    ret.Lock(index, true);
                    continue;
                }
            }

            int periodAreaStart = daysList.IndexOf(scheduleMatrix.EffectivePeriodDays[0]);
            int periodAreaEnd = daysList.IndexOf(scheduleMatrix.EffectivePeriodDays[scheduleMatrix.EffectivePeriodDays.Count - 1]);
            ret.PeriodArea = new MinMax<int>(periodAreaStart, periodAreaEnd);

            return ret;
        }

        public ILockableBitArray CreatePeriodBitArray(bool raise, IScheduleMatrixPro scheduleMatrix)
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
                    continue;
                }
                MinMax<TimeSpan> minMaxLength = _workShiftMinMaxCalculator.PossibleLengthsForDate(scheduleDayPro.Day, scheduleMatrix);
                TimeSpan workingTime = scheduleDayPro.DaySchedulePart().ProjectionService().CreateProjection().ContractTime();
                if(raise && workingTime >= minMaxLength.Maximum)
                {
                    ret.Lock(index, true);
                    continue;
                }
                if(!raise && workingTime <= minMaxLength.Minimum)
                {
                    ret.Lock(index, true);
                    continue;
                }
            }

            int periodAreaStart = daysList.IndexOf(scheduleMatrix.EffectivePeriodDays[0]);
            int periodAreaEnd = daysList.IndexOf(scheduleMatrix.EffectivePeriodDays[scheduleMatrix.EffectivePeriodDays.Count - 1]);
            ret.PeriodArea = new MinMax<int>(periodAreaStart, periodAreaEnd);

            return ret;
        }
    }
}
