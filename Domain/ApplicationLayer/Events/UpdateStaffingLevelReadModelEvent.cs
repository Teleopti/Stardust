namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class UpdateStaffingLevelReadModelEvent : EventWithInfrastructureContext
	{
		public int Days { get; set; }
	}
}