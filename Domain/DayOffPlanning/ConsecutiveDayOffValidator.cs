using System;
using System.Collections;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
    public class ConsecutiveDayOffValidator : IDayOffLegalStateValidator
    {

        #region Variables

        private readonly MinMax<int> _options;
        private readonly bool _considerWeekBefore;
        private readonly bool _considerWeekAfter;
       
        #endregion

        #region Constructor

        public ConsecutiveDayOffValidator(MinMax<int> options, bool considerWeekBefore, bool considerWeekAfter)
        {
            _options = options;
            _considerWeekBefore = considerWeekBefore;
            _considerWeekAfter = considerWeekAfter;
        }

        #endregion

        #region Interface

        public bool IsValid(BitArray periodDays, int dayOffIndex)
        {
            MinMax<int> workingRange = DefineWorkingRange(periodDays);
            int minimumIndex = workingRange.Minimum;
            int maximumIndex = workingRange.Maximum;

            if (dayOffIndex < minimumIndex || dayOffIndex > maximumIndex)
                throw new ArgumentOutOfRangeException("dayOffIndex", "The requested day is not found in the schedule day list");

            if (!periodDays[dayOffIndex])
                throw new ArgumentException("Invalid index. The pointed day is not a day-off", "dayOffIndex");

            int firstDayOffIndex = FindFirstDayOffIndexInBlock(periodDays, dayOffIndex, minimumIndex);
            int dayOffsInBlock = CountDayOffsInBlockLock(periodDays, firstDayOffIndex, maximumIndex);
            return _options.Maximum >= dayOffsInBlock && _options.Minimum <= dayOffsInBlock;
        }

        #endregion

        #region Local

        private static int FindFirstDayOffIndexInBlock(BitArray days, int startIndex, int minIndex)
        {
            int index = startIndex;
            while (index >= minIndex && days[index])
            {
                index--;
            }
            return index + 1;
        }

        private static int CountDayOffsInBlockLock(BitArray days, int firstDayOffIndex, int maxIndex)
        {
            int index = firstDayOffIndex;
            while (index <= maxIndex && days[index])
            {
                index++;
            } 

            return index - firstDayOffIndex;
        }

        private MinMax<int> DefineWorkingRange(BitArray array)
        {
            if (_considerWeekBefore && _considerWeekAfter)
            {
                return new MinMax<int>(0, array.Count - 1);  //scheduleMatrix.OuterWeeksPeriodDays;
            }
            if (_considerWeekBefore)
            {
                return new MinMax<int>(0, array.Count - 8); // scheduleMatrix.WeekBeforeOuterPeriodDays;
            }
            if (_considerWeekAfter)
            {
                return new MinMax<int>(7, array.Count - 1); //scheduleMatrix.WeekAfterOuterPeriodDays;
            }
            return new MinMax<int>(7, array.Count - 8); //scheduleMatrix.FullWeeksPeriodDays;
        }

        #endregion

    }
}