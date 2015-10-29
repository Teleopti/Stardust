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
            TimeSpan dayLightSavingsRecorrection = CalculateDaylightSavingsRecorrection(sourceShiftPeriod, targetShiftPeriod);
            return periodDifference.Add(timeZoneRecorrection).Add(dayLightSavingsRecorrection);
        }

        private static TimeSpan CalculateTimeZoneRecorrection(IScheduleDay target, IScheduleDay source)
        {
            TimeZoneInfo sourceTimeZone = source.TimeZone;
            TimeZoneInfo targetTimeZone = target.TimeZone;
            return targetTimeZone.GetUtcOffset(target.Period.LocalStartDateTime)
                    .Subtract(sourceTimeZone.GetUtcOffset(source.Period.LocalStartDateTime));
        }

		private static TimeSpan CalculateDaylightSavingsRecorrection(DateTimePeriod sourceShiftPeriod, DateTimePeriod targetShiftPeriod)
		{
			var loggedOnPersonsTimezone = StateHolderReader.Instance.StateReader.UserTimeZone;

			var sourceIsDaylightSavingTime = loggedOnPersonsTimezone.IsDaylightSavingTime(sourceShiftPeriod.LocalStartDateTime);
			var targetIsDaylightSavingTime = loggedOnPersonsTimezone.IsDaylightSavingTime(targetShiftPeriod.LocalStartDateTime);

			if (sourceIsDaylightSavingTime == targetIsDaylightSavingTime)
				return TimeSpan.Zero;

			var milliVanilli = -1;
			if (targetShiftPeriod.StartDateTime < sourceShiftPeriod.StartDateTime && targetShiftPeriod.StartDateTime.Month < 7) // spring, could be change to daylight
				milliVanilli = 1;
			if (targetShiftPeriod.StartDateTime == sourceShiftPeriod.StartDateTime)
				milliVanilli = 0;

			var sourceShiftStartTimeInLoggedOnLocal = TimeZoneHelper.ConvertFromUtc(sourceShiftPeriod.StartDateTime, loggedOnPersonsTimezone);
			var targetShiftStartTimeInLoggedOnLocal = TimeZoneHelper.ConvertFromUtc(targetShiftPeriod.StartDateTime, loggedOnPersonsTimezone);

			TimeSpan sourceDaylightOffset =
				loggedOnPersonsTimezone.GetUtcOffset(sourceShiftStartTimeInLoggedOnLocal).Subtract(loggedOnPersonsTimezone.BaseUtcOffset);
			TimeSpan targetDaylightOffset =
				loggedOnPersonsTimezone.GetUtcOffset(targetShiftStartTimeInLoggedOnLocal.AddMilliseconds(milliVanilli)).Subtract((loggedOnPersonsTimezone.BaseUtcOffset));

			return sourceDaylightOffset.Subtract(targetDaylightOffset);

			//if (sourceIsSummerTime)
			//	return TimeSpan.FromHours(1);

			//return TimeSpan.FromHours(-1);
		}

        private static TimeSpan CalculatePeriodDifference(DateTimePeriod sourcePeriod, DateTimePeriod targetPeriod)
        {
                return targetPeriod.StartDateTime.Add(TimeSpan.FromHours(8)).Subtract(sourcePeriod.StartDateTime.Add(TimeSpan.FromHours(8)));
        }
    }
}
