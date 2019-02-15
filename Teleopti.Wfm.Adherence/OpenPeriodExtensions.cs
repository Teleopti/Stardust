using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Wfm.Adherence
{
	public static class OpenPeriodExtensions
	{
		public static IEnumerable<OpenPeriod> Subtract(this IEnumerable<OpenPeriod> periods, IEnumerable<OpenPeriod> toSubtract)
		{
			return toSubtract
				.Aggregate(periods, (ps, toBeSubtracted) =>
					{
						return ps.Aggregate(Enumerable.Empty<OpenPeriod>(), (r, subtractFrom) =>
							{
								Console.WriteLine("Subtract/subtract/toBeSubtracted " + JsonConvert.SerializeObject(toBeSubtracted));
								Console.WriteLine("Subtract/subtract/subtractFrom " + JsonConvert.SerializeObject(subtractFrom));
								var remainder = subtractFrom.subtract(toBeSubtracted);
								Console.WriteLine("Subtract/subtract/remainder " + JsonConvert.SerializeObject(remainder));
								return r.Concat(remainder);
							}
						);
					}
				).ToArray();
		}

		public static IEnumerable<OpenPeriod> SubtractOverlaps(this IEnumerable<OpenPeriod> periods)
//		{
//			Console.WriteLine("SubtractOverlaps");
//			var ret = periods.Aggregate(new List<OpenPeriod>(), (acc, i) =>
//			{
//				Console.WriteLine("SubtractOverlaps/AddRange");
//				var current = i.AsArray().ToArray();
//				Console.WriteLine("SubtractOverlaps/AddRange/current " + JsonConvert.SerializeObject(current));
//				var toSubtract = periods
//					.Where(x => x != i)
//					.ToArray();
//				Console.WriteLine("SubtractOverlaps/AddRange/toSubtract " + JsonConvert.SerializeObject(toSubtract));
//				acc.AddRange(current.Subtract(toSubtract).ToArray());
//				return acc;
//			});
//			Console.WriteLine("/SubtractOverlaps");
//			return ret;
//		}
		
		{
			return periods.OrderBy(x => x.StartTime).Aggregate(new List<OpenPeriod>(), (acc, i) =>
				{
					if (acc.Any() && acc.Last().intersects(i))
						acc.Last().EndTime = acc.Last().EndTime > i.EndTime ? acc.Last().EndTime : i.EndTime;				
					else
						acc.Add(i);							
					
					return acc;
				}
			).ToArray();
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