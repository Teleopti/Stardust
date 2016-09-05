using System;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class StaffingDataSeries
	{
		public DateTime[] Time { get; set; }
		public double[] ForecastedStaffing { get; set; }
		public double?[] UpdatedForecastedStaffing { get; set; }
	}
}