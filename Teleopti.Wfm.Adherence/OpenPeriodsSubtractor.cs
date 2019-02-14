using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Wfm.Adherence
{
	public static class OpenPeriodsSubtractor
	{
		public static IEnumerable<OpenPeriod> Subtract(IEnumerable<OpenPeriod> periods, IEnumerable<OpenPeriod> toSubtract)
		{
			return toSubtract
				.Aggregate(periods, (ps, toBeSubtracted) =>
					{
						return ps.Aggregate(Enumerable.Empty<OpenPeriod>(), (r, recorded) =>
							{
								var remainder = subtract(recorded, toBeSubtracted);
								return r.Concat(remainder);
							}
						);
					}
				).ToArray();
		}

		private static IEnumerable<OpenPeriod> subtract(OpenPeriod subtractFrom, OpenPeriod toSubtract)
		{
			var timePeriods = new List<OpenPeriod>();

			if (subtractFrom.Intersects(toSubtract))
			{
				if (subtractFrom.StartsBefore(toSubtract))
					timePeriods.Add(new OpenPeriod(subtractFrom.StartTime, toSubtract.StartTime));

				if (subtractFrom.EndsAfter(toSubtract))
					timePeriods.Add(new OpenPeriod(toSubtract.EndTime, subtractFrom.EndTime));
			}
			else
			{
				timePeriods.Add(subtractFrom);
			}

			return timePeriods;
		}
	}
}