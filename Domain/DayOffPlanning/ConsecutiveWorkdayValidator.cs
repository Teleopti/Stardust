using System;
using System.Collections;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
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

            MinMax<int> workingRange = defineWorkingRange(periodDays);
            int minimumIndex = workingRange.Minimum;
            int maximumIndex = workingRange.Maximum;

            if (dayOffIndex < minimumIndex || dayOffIndex > maximumIndex)
                throw new ArgumentOutOfRangeException("dayOffIndex", "The requested day is not found in the schedule day list");

            if (!periodDays[dayOffIndex])
                throw new ArgumentException("Invalid index. The pointed day is not a day-off", "dayOffIndex");

            if (!checkWorkdaysBefore(periodDays, dayOffIndex, minimumIndex, maximumIndex)) 
                return false;

            return checkWorkdaysAfter(periodDays, dayOffIndex, maximumIndex);
        }

        #endregion

        #region Local

        private MinMax<int> defineWorkingRange(BitArray array)
        {
	        bool considerWeekBefore = localConsiderWeekBefore(array, _considerWeekBefore);
	        bool considerWeekAfter = localConsiderWeekAfter(array, _considerWeekAfter);

            if (considerWeekBefore && considerWeekAfter)
            {
                return new MinMax<int>(0, array.Count - 1);  //scheduleMatrix.OuterWeeksPeriodDays;
            }
            if (considerWeekBefore)
            {
                return new MinMax<int>(0, array.Count - 8); // scheduleMatrix.WeekBeforeOuterPeriodDays;
            }
            if (considerWeekAfter)
            {
                return new MinMax<int>(7, array.Count - 1); //scheduleMatrix.WeekAfterOuterPeriodDays;
            }
            return new MinMax<int>(7, array.Count - 8); //scheduleMatrix.FullWeeksPeriodDays;
        }

		private static bool localConsiderWeekBefore(BitArray array, bool considerWeekBefore)
		{
			if (!considerWeekBefore)
				return false;

			for (int i = 0; i < 7; i++)
			{
				if (array[i])
					return true;
			}

			return false;
		}

		private static bool localConsiderWeekAfter(BitArray array, bool considerWeekAfter)
		{
			if (!considerWeekAfter)
				return false;

			for (int i = array.Length - 1; i > array.Length - 8; i--)
			{
				if (array[i])
					return true;
			}

			return false;
		}

        private bool checkWorkdaysBefore(BitArray periodDays, int dayOffIndex, int minimumIndex, int maximumIndex)
        {
            int dayIndexBefore = dayOffIndex - 1;
            if (dayIndexBefore < minimumIndex)
                return true;
            if (periodDays[dayIndexBefore])
                return true;
            int firstWorkdayIndex = findFirstWorkdayIndexInBlock(periodDays, dayIndexBefore, minimumIndex);
            int workdaysInBlock = countWorkdaysInBlock(periodDays, firstWorkdayIndex, maximumIndex);
            if (workdaysInBlock == 0)
                return true;
            if (firstWorkdayIndex == minimumIndex)
                return (_options.Maximum >= workdaysInBlock);
            return (_options.Maximum >= workdaysInBlock && _options.Minimum <= workdaysInBlock);
        }

        private bool checkWorkdaysAfter(BitArray periodDays, int dayOffIndex, int maximumIndex)
        {
            int dayIndexAfter = dayOffIndex + 1;
            if (dayIndexAfter > maximumIndex)
                return true;
            if (periodDays[dayIndexAfter])
                return true;
            int lastWorkdayIndex = findLastWorkdayIndexInBlock(periodDays, dayIndexAfter, maximumIndex);
            int workdaysInBlock = countWorkdaysInBlock(periodDays, dayIndexAfter, maximumIndex);
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

        private static int findFirstWorkdayIndexInBlock(BitArray days, int startIndex, int minimumIndex)
        {
            int index = startIndex;
            while (index >= minimumIndex && !days[index])
            {
                index--;
            }

            return index + 1;
        }

        private static int findLastWorkdayIndexInBlock(BitArray days, int startIndex, int maximumIndex)
        {
            int index = startIndex;
            while (index <= maximumIndex && !days[index])
            {
                index++;
            }
            return index - 1;
        }

        private static int countWorkdaysInBlock(BitArray days, int firstWorkdayIndex, int maximumIndex)
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