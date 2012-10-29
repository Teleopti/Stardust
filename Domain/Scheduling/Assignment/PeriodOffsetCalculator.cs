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
        /// <param name="sourceShiftPeriod"></param>
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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
            TimeZoneInfo sourceTimeZone = source.TimeZone;
            TimeZoneInfo targetTimeZone = target.TimeZone;
            return targetTimeZone.GetUtcOffset(target.Period.LocalStartDateTime)
                    .Subtract(sourceTimeZone.GetUtcOffset(source.Period.LocalStartDateTime));
        }

        private static TimeSpan CalculateDaylightSavingsRecorrection(IScheduleDay target, IScheduleDay source, DateTimePeriod sourceShiftPeriod, DateTimePeriod targetShiftPeriod)
        {
            TimeZoneInfo sourceTimeZone = source.TimeZone;
            TimeZoneInfo targetTimeZone = target.TimeZone;

            if (targetShiftPeriod.StartDateTime == sourceShiftPeriod.StartDateTime)
                return TimeSpan.FromHours(0);
            var milliVanilli = -1;
            if (targetShiftPeriod.StartDateTime < sourceShiftPeriod.StartDateTime && targetShiftPeriod.StartDateTime.Month < 7) // spring, could be change to daylight
                milliVanilli = 1;
            if (targetShiftPeriod.StartDateTime == sourceShiftPeriod.StartDateTime)
                milliVanilli = 0;
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
