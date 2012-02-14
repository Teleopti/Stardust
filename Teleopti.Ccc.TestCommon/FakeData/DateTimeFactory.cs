using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Creating test data for DateTime related domain objects
    /// </summary>
    public static class DateTimeFactory
    {
        /// <summary>
        /// Creates the time period.
        /// </summary>
        /// <returns></returns>
        public static DateTimePeriod CreateDateTimePeriod()
        {
            DateTime newDateTime1 = DateTime.MinValue.AddYears(2007);
            DateTime newDateTime2 = DateTime.MinValue.AddYears(2008);
            return CreateDateTimePeriod(newDateTime1, newDateTime2);
        }

        /// <summary>
        /// Creates the time period.
        /// </summary>
        /// <param name="from">From date.</param>
        /// <param name="to">To date.</param>
        /// <returns></returns>
        public static DateTimePeriod CreateDateTimePeriod(DateTime from, DateTime to)
        {
            return new DateTimePeriod(from, to);
        }

        
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimePeriod"/> class.
        /// </summary>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="numberOfDays">The number of days.</param>
        public static DateTimePeriod CreateDateTimePeriod(DateTime startDateTime, int numberOfDays)
        {
            DateTime endDateTime;

            try
            {
                endDateTime = startDateTime.AddDays(numberOfDays + 1).AddTicks(-1L);
            }
            catch (ArgumentOutOfRangeException)
            {
                endDateTime = DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);
            }

            return new DateTimePeriod(startDateTime,endDateTime);
        }

    }
}