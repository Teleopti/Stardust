using System;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ShiftCategoryExample
	{
		public double StartTime { get; set; }
		public double EndTime { get; set; }
		public DayOfWeek DayOfWeek { get; set; }
		public string ShiftCategory { get; set; }
	}
}