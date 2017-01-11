namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class InitialLoadScheduleProjectionEvent : EventWithLogOnContext
	{
		public int StartDays { get; set; }
		public int EndDays { get; set; }
	}
}