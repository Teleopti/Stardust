using Teleopti.Ccc.Domain.Common.EntityBaseTypes;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ISeatPlan : IAggregateRoot, IPublishEvents
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