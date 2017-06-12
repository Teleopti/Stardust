namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class UpdateStaffingLevelReadModel2WeeksEvent : EventWithInfrastructureContext
	{
		public int Days { get; set; }
	}
}