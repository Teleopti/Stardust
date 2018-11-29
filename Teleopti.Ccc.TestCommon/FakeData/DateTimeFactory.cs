using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class DateTimeFactory
    {
		public static DateTimePeriod CreateDateTimePeriodUtc()
		{
			DateTime newDateTime1 = DateTime.Today.ToUniversalTime();
			DateTime newDateTime2 = DateTime.Today.AddHours(12).ToUniversalTime();
			return CreateDateTimePeriod(newDateTime1, newDateTime2);
		}

        public static DateTimePeriod CreateDateTimePeriod(DateTime from, DateTime to)
        {
            return new DateTimePeriod(from, to);
        }

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