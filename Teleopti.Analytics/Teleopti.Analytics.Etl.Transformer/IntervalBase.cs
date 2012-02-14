using System;

namespace Teleopti.Analytics.Etl.Transformer
{
    public class IntervalBase
    {
        private readonly int IntervalsPerDay;

        protected int MinutesPerInterval { private set; get; }

        public IntervalBase(DateTime date, int intervalsPerDay)
        {
            IntervalsPerDay = intervalsPerDay;
            MinutesPerInterval = 1440 / IntervalsPerDay;
            Id = GetIdFromDateTime(date);
        }

        public IntervalBase(int id, int intervalsPerDay)
        {
            IntervalsPerDay = intervalsPerDay;
            MinutesPerInterval = 1440 / IntervalsPerDay;
            Id = id;
        }

        public int Id { get; private set; }

        private int GetIdFromDateTime(DateTime date)
        {
            double minutesElapsedOfDay = date.TimeOfDay.TotalMinutes;
            int id = (int)minutesElapsedOfDay / MinutesPerInterval;

            return id;
        }
    }
}