using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces
{
	public struct SchedulePeriodForRangeCalculation
	{
		public CultureInfo Culture { get; set; }
		public SchedulePeriodType PeriodType { get; set; }
		public DateOnly StartDate { get; set; }
		public int Number { get; set; }
	}
}