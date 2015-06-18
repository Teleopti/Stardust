using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface ISeatPlanRepository : IRepository<ISeatPlan>, ILoadAggregateFromBroker<ISeatPlan>
	{
		void Update (ISeatPlan existingSeatPlan);
		ISeatPlan GetSeatPlanForDate (DateOnly date);
	}
}