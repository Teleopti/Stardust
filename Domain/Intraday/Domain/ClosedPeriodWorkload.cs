using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Intraday.Domain
{
	public class ClosedPeriodWorkload
	{
		public ClosedPeriodWorkload(DateTime start, DateTime end, bool flag)
		{
			Period = new DateTimePeriod(start, end);
			hasBacklogStart = flag;
		}
		public DateTimePeriod Period { get; set; }
		public bool hasBacklogStart { get; set; }
	}
}