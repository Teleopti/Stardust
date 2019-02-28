using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Wfm.Adherence
{
	public static class OpenPeriodExtensions
	{
		public static IEnumerable<OpenPeriod> Subtract(this IEnumerable<OpenPeriod> periods, IEnumerable<OpenPeriod> toSubtract)
		{
			var result = periods.ToArray().AsEnumerable();
			toSubtract.ForEach(s => { result = result.subtract(s); });
			return result;
		}

		public static OpenPeriod Intersection(this OpenPeriod instance, OpenPeriod period)
		{
			if (!instance.intersects(period)) return null;

			var start = instance.startsBefore(period) ? period.StartTime : instance.StartTime;
			var end = instance.endsAfter(period) ? period.EndTime : instance.EndTime;
			return new OpenPeriod(start, end);
		}

		private static IEnumerable<OpenPeriod> subtract(this IEnumerable<OpenPeriod> subtractFroms, OpenPeriod toSubtract)
		{
			var result = new List<OpenPeriod>();
			subtractFroms.ForEach(x => { result.AddRange(x.subtract(toSubtract)); });
			return result;
		}

		private static IEnumerable<OpenPeriod> subtract(this OpenPeriod subtractFrom, OpenPeriod toSubtract)
		{
			var timePeriods = new List<OpenPeriod>();
			if (subtractFrom.intersects(toSubtract))
			{
				if (subtractFrom.startsBefore(toSubtract))
					timePeriods.Add(new OpenPeriod(subtractFrom.StartTime, toSubtract.StartTime));

				if (subtractFrom.endsAfter(toSubtract))
					timePeriods.Add(new OpenPeriod(toSubtract.EndTime, subtractFrom.EndTime));
			}
			else
			{
				timePeriods.Add(new OpenPeriod(subtractFrom.StartTime, subtractFrom.EndTime));
			}

			return timePeriods;
		}

		private static bool intersects(this OpenPeriod instance, OpenPeriod period)
		{
			var startsAfterPeriodEnds = instance.StartTime > period.EndTime;

			var endsBeforePeriodStarts = instance.EndTime < period.StartTime;
			return !(startsAfterPeriodEnds || endsBeforePeriodStarts);
		}

		private static bool startsBefore(this OpenPeriod instance, OpenPeriod period)
		{
			if (instance.StartTime == null && period.StartTime != null)
				return true;
			return instance.StartTime < period.StartTime;
		}

		private static bool endsAfter(this OpenPeriod instance, OpenPeriod period)
		{
			if (instance.EndTime == null && period.EndTime != null)
				return true;
			return instance.EndTime > period.EndTime;
		}
	}
}