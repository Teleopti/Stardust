namespace Teleopti.Wfm.Api.Query
{
	public class ScheduleChangesListenerDto
	{
		public string Url { get; set; }
		public string Name { get; set; }
		public int DaysStartFromCurrentDate { get; set; }
		public int DaysEndFromCurrentDate { get; set; }
	}
}