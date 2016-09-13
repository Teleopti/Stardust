using System;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.Analytics
{
	public class AnalyticsDatePartial : IAnalyticsDate
	{
		public virtual int DateId { get; set; }
		public virtual DateTime DateDate { get; set; }
	}
	public class AnalyticsDate : AnalyticsDatePartial
	{
		public virtual int Year { get; set; }
		public virtual int YearMonth { get; set; }
		public virtual int Month { get; set; }
		public virtual string MonthName { get; set; }
		public virtual string MonthResourceKey { get; set; }
		public virtual int DayInMonth { get; set; }
		public virtual int WeekdayNumber { get; set; }
		public virtual string WeekdayName { get; set; }
		public virtual string WeekdayResourceKey { get; set; }
		public virtual int WeekNumber { get; set; }
		public virtual string YearWeek { get; set; }
		public virtual string Quarter { get; set; }
		public virtual DateTime InsertDate { get; set; }

		public static IAnalyticsDate Eternity = new AnalyticsDate { DateDate = new DateTime(2059, 12, 31), DateId = -2 };
		public static IAnalyticsDate NotDefined = new AnalyticsDate { DateDate = new DateTime(1900, 01, 01), DateId = -1 };
	}
}
