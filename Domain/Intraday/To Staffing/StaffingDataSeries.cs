using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class StaffingDataSeries
	{
		public StaffingDataSeries()
		{
			ForecastedStaffing = new double?[] { };
			UpdatedForecastedStaffing = new double?[] { };
			ActualStaffing = new double?[] { };
			ScheduledStaffing = new double?[] { };
			AbsoluteDifference = new double?[] { };
 		}

		public DateOnly Date { get; set; }
		public DateTime[] Time { get; set; }
		public double?[] ForecastedStaffing { get; set; }
		public double?[] UpdatedForecastedStaffing { get; set; }
		public double?[] ActualStaffing { get; set; }
		public double?[] ScheduledStaffing { get; set; }
		public double?[] AbsoluteDifference { get; set; }

	}
}