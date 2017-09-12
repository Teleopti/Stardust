namespace Teleopti.Ccc.Domain.Analytics
{
	public class AnalyticsTimeZone
	{
		public int TimeZoneId { get; set; }
		public string TimeZoneCode { get; set; }

		public bool IsUtcInUse { get; set; }

		public bool ToBeDeleted { get; set; }
	}
}