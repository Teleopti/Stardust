using System;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.Analytics
{
	public class AnalyticsDate : IAnalyticsDate
	{
		public int DateId { get; set; }
		public DateTime DateDate { get; set; }
	}
}
