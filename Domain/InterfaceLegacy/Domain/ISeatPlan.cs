namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ISeatPlan : IAggregateRootWithEvents
	{
		DateOnly Date { get; set; }
		SeatPlanStatus Status { get; set; }
	}

	public enum SeatPlanStatus
	{
		Ok =0,
		InProgress =1,
		InError =2
	};
}