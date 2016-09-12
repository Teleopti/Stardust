
using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsIntervalRepository
	{
		int IntervalsPerDay();
		AnalyticsInterval MaxInterval();
		IList<AnalyticsInterval> GetAll();
	}

	public class AnalyticsInterval
	{
		public int IntervalId { get; set; }
		public DateTime IntervalStart { get; set; }
		public TimeSpan Offset => IntervalStart.TimeOfDay;
	}
}