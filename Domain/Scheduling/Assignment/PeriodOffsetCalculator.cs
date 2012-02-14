using System;
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
        /// <returns></returns>
        TimeSpan CalculatePeriodOffset(IScheduleDay source, IScheduleDay target, bool ignoreTimeZoneChanges);

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
        
        /// <summary>
        /// Calculates offset (timechange) between the source and the target period.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="ignoreTimeZoneChanges">if set to <c>true</c> ignores the time zone changes, but not the daylight changes.</param>
        /// <returns></returns>
        public TimeSpan CalculatePeriodOffset(IScheduleDay source, IScheduleDay target, bool ignoreTimeZoneChanges)
        {
            if (ignoreTimeZoneChanges)
                return CalculatePeriodOffsetWithoutTimeZoneChanges(source, target);
            return CalculatePeriodOffsetWithTimeZoneChanges(source.Period, target.Period);
        }

        private static TimeSpan CalculatePeriodOffsetWithTimeZoneChanges(DateTimePeriod sourcePeriod, DateTimePeriod targetPeriod)
        {
            return CalculatePeriodDifference(sourcePeriod, targetPeriod);
        }

        private static TimeSpan CalculatePeriodOffsetWithoutTimeZoneChanges(IScheduleDay source, IScheduleDay target)
        {
            TimeSpan periodDifference = CalculatePeriodDifference(source.Period, target.Period);
            TimeSpan timeZoneRecorrection = CalculateTimeZoneRecorrection(target, source);
            TimeSpan dayLightSavingsRecorrection = CalculateDaylightSavingsRecorrection(target, source);
            return periodDifference.Add(timeZoneRecorrection).Add(dayLightSavingsRecorrection);
        }

        private static TimeSpan CalculateTimeZoneRecorrection(IScheduleDay target, IScheduleDay source)
        {
            ICccTimeZoneInfo sourceTimeZone = source.TimeZone;
            ICccTimeZoneInfo targetTimeZone = target.TimeZone;
            return targetTimeZone.GetUtcOffset(target.Period.LocalStartDateTime)
                    .Subtract(sourceTimeZone.GetUtcOffset(source.Period.LocalStartDateTime));
        }

        private static TimeSpan CalculateDaylightSavingsRecorrection(IScheduleDay target, IScheduleDay source)
        {
            ICccTimeZoneInfo sourceTimeZone = source.TimeZone;
            ICccTimeZoneInfo targetTimeZone = target.TimeZone;

            TimeSpan sourceDaylightOffset =
                sourceTimeZone.GetUtcOffset(source.Period.LocalStartDateTime).Subtract(sourceTimeZone.BaseUtcOffset);
            TimeSpan targetDaylightOffset =
                targetTimeZone.GetUtcOffset(target.Period.LocalStartDateTime).Subtract((targetTimeZone.BaseUtcOffset));

            return sourceDaylightOffset.Subtract(targetDaylightOffset);
        }

        private static TimeSpan CalculatePeriodDifference(DateTimePeriod sourcePeriod, DateTimePeriod targetPeriod)
        {
            if (targetPeriod.StartDateTime != sourcePeriod.StartDateTime || targetPeriod.EndDateTime != sourcePeriod.EndDateTime)
            {
                int diffHours = targetPeriod.StartDateTime.TimeOfDay.Subtract(targetPeriod.EndDateTime.TimeOfDay).Hours;
                diffHours -= sourcePeriod.StartDateTime.TimeOfDay.Subtract(sourcePeriod.EndDateTime.TimeOfDay).Hours;
                return targetPeriod.StartDateTime.Subtract(sourcePeriod.StartDateTime.AddHours(diffHours));
            }
            else
                return targetPeriod.StartDateTime.Subtract(sourcePeriod.StartDateTime);
        }
    }
}
