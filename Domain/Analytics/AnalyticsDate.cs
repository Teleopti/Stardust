using System;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.Analytics
{
	public class AnalyticsDate : IAnalyticsDate
	{
		public int DateId { get; set; }
		public DateTime DateDate { get; set; }

		public static IAnalyticsDate Eternity = new AnalyticsDate { DateDate = new DateTime(2059, 12, 31), DateId = -2 };
		public static IAnalyticsDate NotDefined = new AnalyticsDate {DateDate = new DateTime(1900, 01, 01), DateId = -1};
	}
}
