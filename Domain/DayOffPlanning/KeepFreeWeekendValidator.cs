using System;
using System.Collections;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
    public class KeepFreeWeekendValidator : IDayOffLegalStateValidator
    {
    	private readonly MinMax<int> _options;
        private readonly IOfficialWeekendDays _officialWeekendDays;
        private readonly MinMax<int> _periodIndexRange;

    	/// <summary>
        /// Initializes a new instance of the <see cref="KeepFreeWeekendValidator"/> class.
        /// </summary>
        /// <param name="originalPeriodDays">The original period days.</param>
        /// <param name="officialWeekendDays">The official weekend days.</param>
        /// <param name="periodIndexRange">The inner period index range.</param>
        public KeepFreeWeekendValidator(
            BitArray originalPeriodDays,
            IOfficialWeekendDays officialWeekendDays,
            MinMax<int> periodIndexRange)
        {
            _officialWeekendDays = officialWeekendDays;
            _periodIndexRange = periodIndexRange;

            IList<int> weekendDays = WeekendDayIndexes(originalPeriodDays, _periodIndexRange.Minimum, _periodIndexRange.Maximum);
            int weekendCount = CountWeekEnds(weekendDays);
            _options = new MinMax<int>(weekendCount, weekendCount);
        }

    	public bool IsValid(BitArray periodDays, int dayOffIndex)
        {
            int minimumIndex = _periodIndexRange.Minimum;
            int maximumIndex = _periodIndexRange.Maximum;

            if (dayOffIndex < minimumIndex || dayOffIndex > maximumIndex)
                throw new ArgumentOutOfRangeException("dayOffIndex", "The requested day is outside the selected period");

            if (!periodDays[dayOffIndex])
                throw new ArgumentException("Invalid index. The pointed day is not a day-off", "dayOffIndex");


            IList<int> weekendDays = WeekendDayIndexes(periodDays, minimumIndex, maximumIndex);
            int weekendCount = CountWeekEnds(weekendDays);

            return (weekendCount >= _options.Minimum && weekendCount <= _options.Maximum);
        }

    	private static int CountWeekEnds(IList<int> weekendDayIndexes)
        {
            int weekends = 0;
            if (weekendDayIndexes.Count < 2)
                return 0;
            for (int i = 1; i < weekendDayIndexes.Count; i++)
            {
                int daysBetweenTwoWeekendDay = weekendDayIndexes[i] - weekendDayIndexes[i - 1];
                if (daysBetweenTwoWeekendDay == 1)
                    weekends++;
            }
            return weekends;
        }

        private IList<int> WeekendDayIndexes(BitArray array, int minimumIndex, int maximumIndex)
        {
            IList<int> result = new List<int>();
            var officialWeekendDays = _officialWeekendDays.WeekendDayIndexesRelativeStartDayOfWeek();

            for (int i = minimumIndex; i <= maximumIndex; i++)
            {
                int weekPosition = i%7;
                if (officialWeekendDays.Contains(weekPosition) && array[i])
                    result.Add(i);
            }
            return result;
        }
    }
}