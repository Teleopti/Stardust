using System;

namespace Teleopti.Ccc.Intraday.TestApplication
{
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