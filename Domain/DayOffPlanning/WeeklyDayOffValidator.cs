using System;
using System.Collections;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
    public class WeeklyDayOffValidator: IDayOffLegalStateValidator
    {

        #region Variables

        private readonly MinMax<int> _options;

        #endregion

        #region Constructor

        public WeeklyDayOffValidator(MinMax<int> options)
        {
            _options = options;
        }

        #endregion

        #region Interface

        public bool IsValid(BitArray periodDays, int dayOffIndex)
        {
            MinMax<int> workingRange = new MinMax<int>(7, periodDays.Count - 8); 
            int minimumIndex = workingRange.Minimum;
            int maximumIndex = workingRange.Maximum;

            if (dayOffIndex < minimumIndex || dayOffIndex > maximumIndex)
                throw new ArgumentOutOfRangeException("dayOffIndex", "The requested day is not found in the schedule day list");

            if (!periodDays[dayOffIndex])
                throw new ArgumentException("Invalid index. The pointed day is not a day-off", "dayOffIndex");

            int weekFirstDayIndex = GetWeekFirstDayIndex(dayOffIndex);
            int dayOffs = DayOffsOnWeek(periodDays, weekFirstDayIndex, minimumIndex, maximumIndex);
            return dayOffs <= _options.Maximum && dayOffs >= _options.Minimum;
        }

        #endregion

        #region Local

        private static int GetWeekFirstDayIndex(int dayIndex)
        {
            int fullNumberDivider = dayIndex / 7;
            int firstDayIndex = fullNumberDivider * 7;
            return firstDayIndex;
        }

        private static int DayOffsOnWeek(BitArray days, int firstDateOfWeekIndex, int minimumIndex, int maximumIndex)
        {
            int dayOffCount = 0;
            int startIndex = Math.Max(firstDateOfWeekIndex, minimumIndex);
            int endIndex = Math.Min(firstDateOfWeekIndex + 6, maximumIndex);
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (days[i])
                    dayOffCount++;
            }
            return dayOffCount;
        }

        #endregion
    }
}