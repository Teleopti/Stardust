using System;

namespace Teleopti.Ccc.Domain.Common
{
	public class TimeSpanExtensions
	{
		public static TimeSpan? TakeMax(TimeSpan? timespan1, TimeSpan? timespan2)
		{
			if (!timespan1.HasValue)
				return timespan2;
			if (!timespan2.HasValue)
				return timespan1;
			return timespan1 > timespan2 ? timespan1 : timespan2;
		}
		
		public static TimeSpan? TakeMin(TimeSpan? timespan1, TimeSpan? timespan2)
		{
			if (!timespan1.HasValue)
				return timespan2;
			if (!timespan2.HasValue)
				return timespan1;
			return timespan1 < timespan2 ? timespan1 : timespan2;
		}
	}
}