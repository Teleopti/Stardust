using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class Interval : IntervalBase
	{
		public Interval(DateTime date, int intervalsPerDay)
			: base(date, intervalsPerDay)
		{
			Period = createPeriodFromId();
			HalfHourName = getHalfHourName();
			HourName = getHourName();
			IntervalName = getIntervalName();
		}

		public Interval(int id, int intervalsPerDay)
			: base(id, intervalsPerDay)
		{
			Period = createPeriodFromId();
			HalfHourName = getHalfHourName();
			HourName = getHourName();
			IntervalName = getIntervalName();
		}

		public string HalfHourName { get; private set; }

		private string getHalfHourName()
		{
			var startDateTime = new DateTime(1900, 1, 1, Period.StartDateTime.Hour, 0, 0, DateTimeKind.Utc);
			var firstHalfhour = new DateTimePeriod(startDateTime, startDateTime.AddMinutes(30));
			var secondHalfhour = firstHalfhour.MovePeriod(TimeSpan.FromMinutes(30));

			bool isFirstHalfOfHour = firstHalfhour.Intersect(Period);

			var currentHalfhour = isFirstHalfOfHour ? firstHalfhour : secondHalfhour;

			int startHour = currentHalfhour.StartDateTime.Hour;
			int startMinute = currentHalfhour.StartDateTime.Minute;

			int endHour = currentHalfhour.EndDateTime.Hour;
			int endMinute = currentHalfhour.EndDateTime.Minute;


			if (!isFirstHalfOfHour && startHour == 23)
			{
				endHour = 24;
			}

			return
				 String.Concat(getTimeName(startHour, startMinute), "-",
									getTimeName(endHour, endMinute));
		}

		private static string getTimeName(IConvertible hour, IConvertible minute)
		{
			return String.Concat(prefixTimeWithZero(hour), ":", prefixTimeWithZero(minute));
		}

		private static string prefixTimeWithZero(IConvertible timePart)
		{
			return timePart.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0');
		}

		private string getIntervalName()
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
				 String.Concat(getTimeName(startHour, startMinute), "-",
									getTimeName(endHour, endMinute));
		}

		private string getHourName()
		{
			int fromHour = Period.StartDateTime.Hour;
			int toHour;
			if (fromHour == 23)
				toHour = 24;
			else
				toHour = Period.StartDateTime.AddHours(1).Hour;

			return String.Concat(getTimeName(fromHour, 0), "-", getTimeName(toHour, 0));
		}

		public string HourName { get; private set; }
		public string IntervalName { get; private set; }
		public DateTimePeriod Period { get; private set; }

		private DateTimePeriod createPeriodFromId()
		{
			int minutesElapsedOfDay = Id * MinutesPerInterval;

			var periodStart = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			periodStart = periodStart.AddMinutes(minutesElapsedOfDay);
			var periodEnd = periodStart.AddMinutes(MinutesPerInterval);
			var newPeriod = new DateTimePeriod(periodStart, periodEnd);

			return newPeriod;
		}
	}
}