namespace Teleopti.Interfaces.Domain
{
	public interface ISeatPlan : IAggregateRootWithEvents
	{
		DateOnly Date { get; set; }
		SeatPlanStatus Status { get; set; }
	}

	public enum SeatPlanStatus
	{
		Ok,
		InProgress,
		InError
	};
}