using System;
using System.Collections;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
    public class FreeWeekendDayValidator : IDayOffLegalStateValidator
    {
       
        #region Variables

        private readonly MinMax<int> _options;
        private readonly MinMax<int> _periodIndexRange;
        private readonly IOfficialWeekendDays _officialWeekendDays;

        #endregion

        #region Constructor

        public FreeWeekendDayValidator(
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

            IList<int> weekendDays = WeekendDayIndexes(periodDays, minimumIndex, maximumIndex);
            int weekendDayCount = weekendDays.Count;

            return (weekendDayCount >= _options.Minimum && weekendDayCount <= _options.Maximum);
        }

        #endregion

        #region Local

        private IList<int> WeekendDayIndexes(BitArray array, int minimumIndex, int maximumIndex)
        {
            IList<int> result = new List<int>();
            IList<int> officialWeekendDays = _officialWeekendDays.WeekendDayIndexesRelativeStartDayOfWeek();

            for (int i = minimumIndex; i <= maximumIndex; i++)
            {
                int weekPosition = i%7;
                if (officialWeekendDays.IndexOf(weekPosition) > -1 && array[i])
                    result.Add(i);
            }
            return result;
        }
        
        #endregion
    }
}