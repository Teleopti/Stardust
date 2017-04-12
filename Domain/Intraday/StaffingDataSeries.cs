using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class StaffingDataSeries
	{
		public DateOnly Date { get; set; }
		public DateTime[] Time { get; set; }
		public double?[] ForecastedStaffing { get; set; }
		public double?[] UpdatedForecastedStaffing { get; set; }
		public double?[] ActualStaffing { get; set; }
		public double?[] ScheduledStaffing { get; set; }
	}
}