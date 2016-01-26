using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface ISeatPlanRepository : IRepository<ISeatPlan>
	{
		void RemoveSeatPlanForDate (DateOnly date);
		ISeatPlan GetSeatPlanForDate (DateOnly date);
	}
}