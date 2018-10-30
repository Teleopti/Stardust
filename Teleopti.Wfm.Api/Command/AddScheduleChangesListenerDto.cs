namespace Teleopti.Wfm.Api.Command
{
	public class AddScheduleChangesListenerDto : ICommandDto
	{
		public string Url { get; set; }
		public string Name { get; set; }
		public int DaysStartFromCurrentDate { get; set; }
		public int DaysEndFromCurrentDate { get; set; }
	}
}