using System;
using System.Collections;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanning
{
    public class ConsecutiveWorkdayValidator : IDayOffLegalStateValidator
    {

        #region Variables

        private readonly MinMax<int> _options;
        private readonly bool _considerWeekBefore;
        private readonly bool _considerWeekAfter;

        #endregion

        #region Constructor

        public ConsecutiveWorkdayValidator(MinMax<int> options, bool considerWeekBefore, bool considerWeekAfter)
        {
            _options = options;
            _considerWeekBefore = considerWeekBefore;
            _considerWeekAfter = considerWeekAfter;
        }

        #endregion

        #region Interface

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public bool IsValid(BitArray periodDays, int dayOffIndex)
        {

            MinMax<int> workingRange = DefineWorkingRange(periodDays);
            int minimumIndex = workingRange.Minimum;
            int maximumIndex = workingRange.Maximum;

            if (dayOffIndex < minimumIndex || dayOffIndex > maximumIndex)
                throw new ArgumentOutOfRangeException("dayOffIndex", "The requested day is not found in the schedule day list");

            if (!periodDays[dayOffIndex])
                throw new ArgumentException("Invalid index. The pointed day is not a day-off", "dayOffIndex");

            if (!CheckWorkdaysBefore(periodDays, dayOffIndex, minimumIndex, maximumIndex)) 
                return false;

            return CheckWorkdaysAfter(periodDays, dayOffIndex, maximumIndex);
        }

        #endregion

        #region Local

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

        private bool CheckWorkdaysBefore(BitArray periodDays, int dayOffIndex, int minimumIndex, int maximumIndex)
        {
            int dayIndexBefore = dayOffIndex - 1;
            if (dayIndexBefore < minimumIndex)
                return true;
            if (periodDays[dayIndexBefore])
                return true;
            int firstWorkdayIndex = FindFirstWorkdayIndexInBlock(periodDays, dayIndexBefore, minimumIndex);
            int workdaysInBlock = CountWorkdaysInBlock(periodDays, firstWorkdayIndex, maximumIndex);
            if (workdaysInBlock == 0)
                return true;
            if (firstWorkdayIndex == minimumIndex)
                return (_options.Maximum >= workdaysInBlock);
            return (_options.Maximum >= workdaysInBlock && _options.Minimum <= workdaysInBlock);
        }

        private bool CheckWorkdaysAfter(BitArray periodDays, int dayOffIndex, int maximumIndex)
        {
            int dayIndexAfter = dayOffIndex + 1;
            if (dayIndexAfter > maximumIndex)
                return true;
            if (periodDays[dayIndexAfter])
                return true;
            int lastWorkdayIndex = FindLastWorkdayIndexInBlock(periodDays, dayIndexAfter, maximumIndex);
            int workdaysInBlock = CountWorkdaysInBlock(periodDays, dayIndexAfter, maximumIndex);
            if (workdaysInBlock == 0)
                return true;
            if (lastWorkdayIndex == maximumIndex)
                return (_options.Maximum >= workdaysInBlock);
            return (_options.Maximum >= workdaysInBlock && _options.Minimum <= workdaysInBlock);
        }

        //private static int WorkdaysAfter(BitArray days, int dayOffIndex, int maximumIndex)
        //{
        //    int dayIndexAfter = dayOffIndex + 1;
        //    if (dayIndexAfter > maximumIndex)
        //        return 0;
        //    if (days[dayIndexAfter])
        //        return 0;
        //    int lastWorkdayIndex = FindLastWorkdayIndexInBlock(days, dayIndexAfter, maximumIndex);
        //    if (lastWorkdayIndex == maximumIndex)
        //        return 0;
        //    int result = CountWorkdaysInBlock(days, dayIndexAfter, maximumIndex);
        //    return result;
        //}

        //private bool CheckWorkdays(int countWorkdays)
        //{
        //    if(countWorkdays == 0)
        //        return true;
        //    return (_options.Maximum >= countWorkdays && _options.Minimum <= countWorkdays);
        //}

        private static int FindFirstWorkdayIndexInBlock(BitArray days, int startIndex, int minimumIndex)
        {
            int index = startIndex;
            while (index >= minimumIndex && !days[index])
            {
                index--;
            }

            return index + 1;
        }

        private static int FindLastWorkdayIndexInBlock(BitArray days, int startIndex, int maximumIndex)
        {
            int index = startIndex;
            while (index <= maximumIndex && !days[index])
            {
                index++;
            }
            return index - 1;
        }

        private static int CountWorkdaysInBlock(BitArray days, int firstWorkdayIndex, int maximumIndex)
        {
            int index = firstWorkdayIndex;
            while (index <= maximumIndex && !days[index])
            {
                index++;
            }

            return index - firstWorkdayIndex;
        }
        
        #endregion

    }
}