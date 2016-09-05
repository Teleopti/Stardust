using System;

namespace Teleopti.Ccc.Domain.LogObject
{
	public class HistoricalDataDetail
	{
		public int LogObjectId { get; set; }
		public string LogObjectName { get; set; }
		public int IntervalsPerDay { get; set; }
		public int DetailId { get; set; }
		public string DetailName { get; set; }
		public DateTime DateValue { get; set; }
		public int IntervalValue { get; set; }
	}
}