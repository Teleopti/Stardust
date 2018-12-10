using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy
{
	public struct SchedulePeriodForRangeCalculation
	{
		public CultureInfo Culture { get; set; }
		public SchedulePeriodType PeriodType { get; set; }
		public DateOnly StartDate { get; set; }
		public int Number { get; set; }
	}
}