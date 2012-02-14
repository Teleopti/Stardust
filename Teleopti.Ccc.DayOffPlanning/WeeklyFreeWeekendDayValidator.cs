using System;
using System.Collections;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanning
{
    public class WeeklyFreeWeekendDayValidator : IDayOffLegalStateValidator
    {
       
        #region Variables

        private readonly MinMax<int> _options;
        private readonly MinMax<int> _periodIndexRange;
        private readonly IOfficialWeekendDays _officialWeekendDays;

        #endregion

        #region Constructor

        public WeeklyFreeWeekendDayValidator(
            MinMax<int> options,
            IOfficialWeekendDays officialWeekendDays,
            MinMax<int> periodIndexRange)
        {
            _options = options;
            _officialWeekendDays = officialWeekendDays;
            _periodIndexRange = periodIndexRange;
        }

        #endregion

        #region Interface

        public bool IsValid(BitArray periodDays, int dayOffIndex)
        {
            int minimumIndex = _periodIndexRange.Minimum;
            int maximumIndex = _periodIndexRange.Maximum;

            if (dayOffIndex < minimumIndex || dayOffIndex > maximumIndex)
                throw new ArgumentOutOfRangeException("dayOffIndex", "The requested day is outside the selected period");

            if (!periodDays[dayOffIndex])
                throw new ArgumentException("Invalid index. The pointed day is not a day-off", "dayOffIndex");

            MinMax<int> fullWeekPeriodRangeIndex = FullWeekPeriodRangeIndex();
            int weeks = CalculateWeekCount(fullWeekPeriodRangeIndex);
            
            for (int i = 0; i < weeks; i++)
            {
                int weekStartIndex = fullWeekPeriodRangeIndex.Minimum + 7*i;
                if (!IsWeekValid(periodDays, weekStartIndex))
                    return false;
            }
            return true;
        }

        private static int CalculateWeekCount(MinMax<int> fullWeekPeriodRangeIndex)
        {
            return (fullWeekPeriodRangeIndex.Maximum - fullWeekPeriodRangeIndex.Minimum + 1) / 7;
        }

        private MinMax<int> FullWeekPeriodRangeIndex()
        {
            int minimumIndex = _periodIndexRange.Minimum - _periodIndexRange.Minimum % 7;
            int maximumIndex = _periodIndexRange.Maximum + 6 - _periodIndexRange.Maximum % 7;
            return new MinMax<int>(minimumIndex, maximumIndex);
        }

        #endregion

        #region Local

        private IList<int> WeekendDayIndexes(BitArray periodDays, int minimumIndex, int maximumIndex)
        {
            IList<int> result = new List<int>();
            IList<int> officialWeekendDays = _officialWeekendDays.WeekendDayIndexes();

            for (int i = minimumIndex; i <= maximumIndex; i++)
            {
                int weekPosition = i%7;
                if (officialWeekendDays.IndexOf(weekPosition) > -1 && periodDays[i])
                    result.Add(i);
            }
            return result;
        }

        private bool IsWeekValid(BitArray periodDays, int startIndex)
        {
            int endIndex = startIndex + 6;

            IList<int> weekendDays = WeekendDayIndexes(periodDays, startIndex, endIndex);
            int weekendDayCount = weekendDays.Count;

            return (weekendDayCount >= _options.Minimum && weekendDayCount <= _options.Maximum);
        }

        #endregion
    }
}