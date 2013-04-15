namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class DailyStaffingMetricsViewModel
	{
		public double ForecastedHours { get; set; }

		public double ESL { get; set; }

		public double ScheduledHours { get; set; }

		public double RelativeDifference { get; set; }

		public double AbsoluteDifferenceHours { get; set; }
	}
}