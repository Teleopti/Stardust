using System;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer
{
    public class Interval : IntervalBase
    {
        public Interval(DateTime date, int intervalsPerDay) : base(date, intervalsPerDay)
        {
            Period = CreatePeriodFromId();
            HalfHourName = GetHalfHourName();
            HourName = GetHourName();
            IntervalName = GetIntervalName();
        }

        public Interval(int id, int intervalsPerDay) : base(id, intervalsPerDay)
        {
            Period = CreatePeriodFromId();
            HalfHourName = GetHalfHourName();
            HourName = GetHourName();
            IntervalName = GetIntervalName();
        }

        public string HalfHourName { get; private set; }

        private string GetHalfHourName()
        {
            DateTime startDateTime = new DateTime(1900, 1, 1, Period.StartDateTime.Hour, 0, 0, DateTimeKind.Utc);
            DateTimePeriod firstHalfhour = new DateTimePeriod(startDateTime, startDateTime.AddMinutes(30));
            DateTimePeriod secondHalfhour =firstHalfhour.MovePeriod(TimeSpan.FromMinutes(30));

            bool isFirstHalfOfHour = firstHalfhour.Intersect(Period);

            DateTimePeriod currentHalfhour = isFirstHalfOfHour ? firstHalfhour : secondHalfhour;

            int startHour = currentHalfhour.StartDateTime.Hour;
            int startMinute = currentHalfhour.StartDateTime.Minute;

            int endHour = currentHalfhour.EndDateTime.Hour;
            int endMinute = currentHalfhour.EndDateTime.Minute;


            if (!isFirstHalfOfHour && startHour == 23)
            {
                endHour = 24;
            }

            return
                String.Concat(GetTimeName(startHour, startMinute), "-",
                              GetTimeName(endHour, endMinute));
        }

        private static string GetTimeName(IConvertible hour, IConvertible minute)
        {
            return String.Concat(PrefixTimeWithZero(hour), ":", PrefixTimeWithZero(minute));
        }

        private static string PrefixTimeWithZero(IConvertible timePart)
        {
            return timePart.ToString(CultureInfo.InvariantCulture).PadLeft(2,'0');
        }

        private string GetIntervalName()
        {
            int startHour = Period.StartDateTime.Hour;
            int startMinute = Period.StartDateTime.Minute;

            int endHour = Period.EndDateTime.Hour;
            int endMinute = Period.EndDateTime.Minute;

            if (endHour == 0 && endMinute == 0)
            {
                endHour = 24;
            }

            return
                String.Concat(GetTimeName(startHour, startMinute), "-",
                              GetTimeName(endHour, endMinute));
        }

        private string GetHourName()
        {
            int fromHour = Period.StartDateTime.Hour;
            int toHour;
            if (fromHour == 23)
                toHour = 24;
            else
                toHour = Period.StartDateTime.AddHours(1).Hour;

            return String.Concat(GetTimeName(fromHour, 0), "-", GetTimeName(toHour, 0));
        }

        public string HourName { get; private set; }
        public string IntervalName { get; private set; }
        public DateTimePeriod Period { get; private set; }

        private DateTimePeriod CreatePeriodFromId()
        {
            int minutesElapsedOfDay = Id*MinutesPerInterval;

            DateTime periodStart = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            periodStart = periodStart.AddMinutes(minutesElapsedOfDay);
            DateTime periodEnd = periodStart.AddMinutes(MinutesPerInterval);
            DateTimePeriod newPeriod = new DateTimePeriod(periodStart, periodEnd);

            return newPeriod;
        }
    }
}