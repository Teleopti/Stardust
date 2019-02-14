using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Wfm.Adherence
{
	public static class OpenPeriodsSubtractor
	{
		public static IEnumerable<OpenPeriod> Subtract(IEnumerable<OpenPeriod> periods, IEnumerable<OpenPeriod> toSubtract)
		{
			return toSubtract
				.Aggregate(periods, (ps, approved) =>
					{
						return ps.Aggregate(Enumerable.Empty<OpenPeriod>(), (r, recorded) =>
							{
								var subtracted = subtract(recorded, approved);
								return r.Concat(subtracted);
							}
						);
					}
				).ToArray();
		}

		private static IEnumerable<OpenPeriod> subtract(OpenPeriod subtractFrom, OpenPeriod toSubtract)
		{
			var timePeriods = new List<OpenPeriod>();

			if (notIntersecting(subtractFrom, toSubtract))
				timePeriods.Add(subtractFrom);
			else
			{
				if (subtractFrom.StartTime == null || subtractFrom.StartTime < toSubtract.StartTime)
				{
					var leftTimePeriod = new OpenPeriod(subtractFrom.StartTime, toSubtract.StartTime);
					timePeriods.Add(leftTimePeriod);

					if (subtractFrom.EndTime == null || subtractFrom.EndTime > toSubtract.EndTime)
					{
						var rightTimePeriod = new OpenPeriod(toSubtract.EndTime, subtractFrom.EndTime);
						timePeriods.Add(rightTimePeriod);
					}
				}
				else if (subtractFrom.EndTime == null || subtractFrom.EndTime > toSubtract.EndTime)
				{
					var rightTimePeriod = new OpenPeriod(toSubtract.EndTime, subtractFrom.EndTime);
					timePeriods.Add(rightTimePeriod);
				}
			}

			return timePeriods;
		}

		private static bool notIntersecting(OpenPeriod firstPeriod, OpenPeriod secondPeriod)
		{
			var startsAfterLastPeriodEnds = firstPeriod.StartTime > secondPeriod.EndTime;
			var endsBeforeLastPeriodStarts = firstPeriod.EndTime < secondPeriod.StartTime;
			return startsAfterLastPeriodEnds || endsBeforeLastPeriodStarts;
		}
	}
}