using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Wfm.Adherence
{
	public static class OpenPeriodExtensions
	{
		public static IEnumerable<OpenPeriod> Subtract(this IEnumerable<OpenPeriod> periods, IEnumerable<OpenPeriod> toSubtract)
		{
			var result = periods.ToArray().AsEnumerable();
			toSubtract.ForEach(s =>
			{
				result = result.subtract(s);
			});
			return result;
		}

		private static IEnumerable<OpenPeriod> subtract(this IEnumerable<OpenPeriod> subtractFroms, OpenPeriod toSubtract)
		{
			var result = new List<OpenPeriod>();
			subtractFroms.ForEach(x =>
			{
				result.AddRange(x.subtract(toSubtract));
			});
			return result;
		}

		private static IEnumerable<OpenPeriod> subtract(this OpenPeriod subtractFrom, OpenPeriod toSubtract)
		{
			var timePeriods = new List<OpenPeriod>();
			if (subtractFrom.Intersects(toSubtract))
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

		public static IEnumerable<OpenPeriod> MergeIntersecting(this IEnumerable<OpenPeriod> periods)
		{
			var result = new List<OpenPeriod>();
			periods
				.OrderBy(x => x.StartTime)
				.ForEach(x =>
				{
					if (result.Any() && result.Last().Intersects(x))
						result.Last().EndTime = new[] {x.EndTime, result.Last().EndTime}.Max();
					else
						result.Add(x);
				});
			return result;
		}

		public static bool Intersects(this OpenPeriod instance, OpenPeriod period)
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