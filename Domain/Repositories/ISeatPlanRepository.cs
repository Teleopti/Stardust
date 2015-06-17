using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface ISeatPlanRepository : IRepository<ISeatPlan>, ILoadAggregateFromBroker<ISeatPlan>
	{
		
	}
}