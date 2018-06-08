using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Intraday.TestCommon
{
	public class IntradayTestInput
	{
		public UserTimeZoneInfo UserTimeZoneInfo { get; set; }
		public TimeZoneInterval TimeZoneInterval { get; set; }
		public DateTime Date { get; set; }
		public List<IntradayTestDateTimePeriod> TimePeriods { get; set; }
	}

	public class IntradayTestDateTimePeriod
	{
		public IntradayTestDateTimePeriod(DateTime from, DateTime to)
		{
			From = from;
			To = to;
		}

		public DateTime From { get; set; }
		public DateTime To { get; set; }
	}
}