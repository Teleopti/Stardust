using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Intraday.TestApplication
{
	public class IntradayTestApplicationInput
	{
		public UserTimeZoneInfo UserTimeZoneInfo { get; set; }
		public TimeZoneInterval TimeZoneInterval { get; set; }
		public DateTime Date { get; set; }
		public List<IntradayTestDateTimePeriod> TimePeriods { get; set; }
	}
}