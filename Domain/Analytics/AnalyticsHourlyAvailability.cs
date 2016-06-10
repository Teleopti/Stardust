namespace Teleopti.Ccc.Domain.Analytics
{
	public class AnalyticsHourlyAvailability
	{
		public int DateId { get; set; }
		public int PersonId { get; set; }
		public int ScenarioId { get; set; }
		public int AvailableTimeMinutes { get; set; }
		public int AvailableDays { get; set; }
		public int ScheduledTimeMinutes { get; set; }
		public int ScheduledDays { get; set; }
		public int BusinessUnitId { get; set; }
		public int DatasourceId { get; set; }
	}
}