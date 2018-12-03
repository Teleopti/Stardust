using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    /// <summary>
    /// Calculator to calculate the offset (timechange) between the source and the target period.
    /// </summary>
    public class PeriodOffsetCalculator
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

		public TimeSpan CalculatePeriodOffsetConsiderDaylightSavings(IScheduleDay source, IScheduleDay target, DateTimePeriod sourceShiftPeriod)
	    {
			var periodDifference = CalculatePeriodDifference(source.Period, target.Period);
			var targetShiftPeriod = sourceShiftPeriod.MovePeriod(periodDifference);
			var dayLightSavingsRecorrection = CalculateDaylightSavingsRecorrection(sourceShiftPeriod, targetShiftPeriod);
			return periodDifference.Add(dayLightSavingsRecorrection);
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
            var periodDifference = CalculatePeriodDifference(source.Period, target.Period);
            var timeZoneRecorrection = target.TimeZone.GetUtcOffset(target.Period.StartDateTime)
					.Subtract(source.TimeZone.GetUtcOffset(source.Period.StartDateTime));
            
            var targetShiftPeriod = sourceShiftPeriod.MovePeriod(periodDifference + timeZoneRecorrection);
            TimeSpan dayLightSavingsRecorrection = CalculateDaylightSavingsRecorrection(sourceShiftPeriod, targetShiftPeriod);
            return periodDifference.Add(timeZoneRecorrection).Add(dayLightSavingsRecorrection);
        }

		private static TimeSpan CalculateDaylightSavingsRecorrection(DateTimePeriod sourceShiftPeriod, DateTimePeriod targetShiftPeriod)
		{
			//pretty sure this method is compeletly wrong/unnecessary... all timezones here don't make sense.
			var currentUserTimeZone = TimeZoneGuard.Instance.CurrentTimeZone();
			
			var sourceIsDaylightSavingTime = currentUserTimeZone.IsDaylightSavingTime(TimeZoneHelper.ConvertFromUtc(sourceShiftPeriod.StartDateTime, currentUserTimeZone));
			var targetIsDaylightSavingTime = currentUserTimeZone.IsDaylightSavingTime(TimeZoneHelper.ConvertFromUtc(targetShiftPeriod.StartDateTime, currentUserTimeZone));

			if (sourceIsDaylightSavingTime == targetIsDaylightSavingTime)
				return TimeSpan.Zero;

			var milliVanilli = -1;
			if (targetShiftPeriod.StartDateTime < sourceShiftPeriod.StartDateTime && targetShiftPeriod.StartDateTime.Month < 7) // spring, could be change to daylight
				milliVanilli = 1;
			if (targetShiftPeriod.StartDateTime == sourceShiftPeriod.StartDateTime)
				milliVanilli = 0;

			var sourceShiftStartTimeInLoggedOnLocal = TimeZoneHelper.ConvertFromUtc(sourceShiftPeriod.StartDateTime, currentUserTimeZone);
			var targetShiftStartTimeInLoggedOnLocal = TimeZoneHelper.ConvertFromUtc(targetShiftPeriod.StartDateTime, currentUserTimeZone);

			TimeSpan sourceDaylightOffset =
				currentUserTimeZone.GetUtcOffset(sourceShiftStartTimeInLoggedOnLocal).Subtract(currentUserTimeZone.BaseUtcOffset);
			TimeSpan targetDaylightOffset =
				currentUserTimeZone.GetUtcOffset(targetShiftStartTimeInLoggedOnLocal.AddMilliseconds(milliVanilli)).Subtract((currentUserTimeZone.BaseUtcOffset));

			return sourceDaylightOffset.Subtract(targetDaylightOffset);
		}

        private static TimeSpan CalculatePeriodDifference(DateTimePeriod sourcePeriod, DateTimePeriod targetPeriod)
        {
                return targetPeriod.StartDateTime.Add(TimeSpan.FromHours(8)).Subtract(sourcePeriod.StartDateTime.Add(TimeSpan.FromHours(8)));
        }
    }
}
