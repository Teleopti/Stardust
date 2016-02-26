using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics.Transformer
{
    public class IntervalBase
    {
	    protected int MinutesPerInterval { private set; get; }

        public IntervalBase(DateTime date, int intervalsPerDay)
        {
	        MinutesPerInterval = 1440 / intervalsPerDay;
            Id = getIdFromDateTime(date);
        }

        public IntervalBase(int id, int intervalsPerDay)
        {
	        MinutesPerInterval = 1440 / intervalsPerDay;
            Id = id;
        }

        public int Id { get; private set; }

        private int getIdFromDateTime(DateTime date)
        {
            double minutesElapsedOfDay = date.TimeOfDay.TotalMinutes;
            int id = (int)minutesElapsedOfDay / MinutesPerInterval;

            return id;
        }
    }
}