using System;
using System.Collections.Generic;

namespace Teleopti.Wfm.Adherence
{
	public struct DateTimePeriod : IEquatable<DateTimePeriod>
	{
		private MinMax<DateTime> period;

		public DateTimePeriod(DateTime startDateTime, DateTime endDateTime)
		{
			validateDateTime(startDateTime, endDateTime);
			period = new MinMax<DateTime>(startDateTime, endDateTime);
		}

		private DateTimePeriod(DateTime startDateTime, DateTime endDateTime, bool validate)
		{
			if (validate)
				validateDateTime(startDateTime, endDateTime);

			period = new MinMax<DateTime>(startDateTime, endDateTime);
		}

		private static void validateDateTime(DateTime startDateTime, DateTime endDateTime)
		{
//			InParameter.VerifyDateIsUtc(nameof(startDateTime), startDateTime);
//			InParameter.VerifyDateIsUtc(nameof(endDateTime), endDateTime);
		}
		
		public DateTime StartDateTime => period.Minimum;
		public DateTime EndDateTime => period.Maximum;

		public TimeSpan ElapsedTime()
		{
			return EndDateTime.Subtract(StartDateTime);
		}

		public bool Intersect(DateTimePeriod intersectPeriod)
		{
			return !((StartDateTime >= intersectPeriod.EndDateTime) || (EndDateTime <= intersectPeriod.StartDateTime));
		}

		public DateTimePeriod? Intersection(DateTimePeriod intersectPeriod)
		{
			if (!Intersect(intersectPeriod))
				return null;

			var intersectStart = intersectPeriod.StartDateTime;
			var intersectEnd = intersectPeriod.EndDateTime;
			var start = period.Minimum;
			if (intersectStart > start)
				start = intersectStart;
			var end = period.Maximum;
			if (intersectEnd < end)
				end = intersectEnd;
			return new DateTimePeriod(start, end, false);
		}

		public override string ToString()
		{
			return StartDateTime + " - " + EndDateTime;
		}

		public IEnumerable<DateTimePeriod> Subtract(DateTimePeriod exclude)
		{
			if (!Intersect(exclude))
				return new[] {this};

			var timePeriods = new List<DateTimePeriod>();
			if (StartDateTime < exclude.StartDateTime)
			{
				var leftTimePeriod = new DateTimePeriod(StartDateTime, exclude.StartDateTime);
				timePeriods.Add(leftTimePeriod);
			}

			if (EndDateTime > exclude.EndDateTime)
			{
				var rightTimePeriod = new DateTimePeriod(exclude.EndDateTime, EndDateTime);
				timePeriods.Add(rightTimePeriod);
			}

			return timePeriods;
		}


		public bool Equals(DateTimePeriod other)
		{
			return other.StartDateTime == StartDateTime &&
				   other.EndDateTime == EndDateTime;
		}

		public override bool Equals(object obj)
		{
			return obj is DateTimePeriod timePeriod && Equals(timePeriod);
		}

		public static bool operator ==(DateTimePeriod per1, DateTimePeriod per2)
		{
			return per1.Equals(per2);
		}

		public static bool operator !=(DateTimePeriod per1, DateTimePeriod per2)
		{
			return !per1.Equals(per2);
		}

		public override int GetHashCode()
		{
			return (period.Minimum.GetHashCode() * 397) ^ period.Maximum.GetHashCode();
		}
	}
}