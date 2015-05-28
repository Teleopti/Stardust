using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public static class IntervalFactory
	{
		public static IList<Interval> CreateIntervalCollection(int intervalsPerDay)
		{
			IList<Interval> retList = new List<Interval>();

			for (int i = 0; i < intervalsPerDay; i++)
			{
				var interval = new Interval(i, intervalsPerDay);
				retList.Add(interval);
			}
			return retList;
		}
	}
}