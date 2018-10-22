using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public static class IntervalFactory
	{
		public static IList<Interval> CreateIntervalCollection(int intervalsPerDay)
		{
			return Enumerable.Range(0,intervalsPerDay).Select(i => new Interval(i, intervalsPerDay)).ToArray();
		}
	}
}