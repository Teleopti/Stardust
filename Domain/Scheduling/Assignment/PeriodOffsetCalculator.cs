﻿using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public interface IPeriodOffsetCalculator
    {
        /// <summary>
        /// Calculates offset (timechange) between the source and the target period.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="ignoreTimeZoneChanges">if set to <c>true</c> [ignore time zone changes].</param>
        /// <param name="sourceShiftPeriod">To know if Daylightsavingtime has changed we must know when the shift starts</param>
        /// <returns></returns>
        TimeSpan CalculatePeriodOffset(IScheduleDay source, IScheduleDay target, bool ignoreTimeZoneChanges, DateTimePeriod sourceShiftPeriod);

        TimeSpan CalculatePeriodOffset(DateTimePeriod sourcePeriod, DateTimePeriod targetPeriod);
    }

    /// <summary>
    /// Calculator to calculate the offset (timechange) between the source and the target period.
    /// </summary>
    public class PeriodOffsetCalculator : IPeriodOffsetCalculator
    {

        /// <summary>
        /// Calculates offset (timechange) between the source and the target period.
        /// </summary>
        /// <param name="sourcePeriod">The source period.</param>
        /// <param name="targetPeriod">The target period.</param>
        /// <returns></returns>
        public TimeSpan CalculatePeriodOffset(DateTimePeriod sourcePeriod, DateTimePeriod targetPeriod)
        {
            return CalculatePeriodOffsetWithTimeZoneChanges(sourcePeriod, targetPeriod);
        }

        public TimeSpan CalculatePeriodOffset(IScheduleDay source, IScheduleDay target, bool ignoreTimeZoneChanges, DateTimePeriod sourceShiftPeriod)
        {
            if (ignoreTimeZoneChanges)
                return CalculatePeriodOffsetWithoutTimeZoneChanges(source, target, sourceShiftPeriod);
            return CalculatePeriodOffsetWithTimeZoneChanges(source.Period, target.Period);
        }

        private static TimeSpan CalculatePeriodOffsetWithTimeZoneChanges(DateTimePeriod sourcePeriod, DateTimePeriod targetPeriod)
        {
            return CalculatePeriodDifference(sourcePeriod, targetPeriod);
        }

        private static TimeSpan CalculatePeriodOffsetWithoutTimeZoneChanges(IScheduleDay source, IScheduleDay target, DateTimePeriod sourceShiftPeriod)
        {
            TimeSpan periodDifference = CalculatePeriodDifference(source.Period, target.Period);
            TimeSpan timeZoneRecorrection = CalculateTimeZoneRecorrection(target, source);
            var targetShiftPeriod = sourceShiftPeriod.MovePeriod(periodDifference + timeZoneRecorrection);
            TimeSpan dayLightSavingsRecorrection = CalculateDaylightSavingsRecorrection(target, source, sourceShiftPeriod, targetShiftPeriod);
            return periodDifference.Add(timeZoneRecorrection).Add(dayLightSavingsRecorrection);
        }

        private static TimeSpan CalculateTimeZoneRecorrection(IScheduleDay target, IScheduleDay source)
        {
            ICccTimeZoneInfo sourceTimeZone = source.TimeZone;
            ICccTimeZoneInfo targetTimeZone = target.TimeZone;
            return targetTimeZone.GetUtcOffset(target.Period.LocalStartDateTime)
                    .Subtract(sourceTimeZone.GetUtcOffset(source.Period.LocalStartDateTime));
        }

        private static TimeSpan CalculateDaylightSavingsRecorrection(IScheduleDay target, IScheduleDay source, DateTimePeriod sourceShiftPeriod, DateTimePeriod targetShiftPeriod)
        {
            ICccTimeZoneInfo sourceTimeZone = source.TimeZone;
            ICccTimeZoneInfo targetTimeZone = target.TimeZone;
            var milliVanilli = 1;
            if (targetShiftPeriod.StartDateTime.Month < 7) // spring, could be change to daylight
                milliVanilli = -1;
            TimeSpan sourceDaylightOffset =
                sourceTimeZone.GetUtcOffset(sourceShiftPeriod.LocalStartDateTime).Subtract(sourceTimeZone.BaseUtcOffset);
            TimeSpan targetDaylightOffset =
                targetTimeZone.GetUtcOffset(targetShiftPeriod.LocalStartDateTime.AddMilliseconds(milliVanilli)).Subtract((targetTimeZone.BaseUtcOffset));

            return sourceDaylightOffset.Subtract(targetDaylightOffset);
        }

        private static TimeSpan CalculatePeriodDifference(DateTimePeriod sourcePeriod, DateTimePeriod targetPeriod)
        {
                return targetPeriod.StartDateTime.Subtract(sourcePeriod.StartDateTime);
        }
    }
}
