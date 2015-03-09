using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface ISeatMapRepository : IRepository<ISeatMap>, IWriteSideRepository<ISeatMap>, ILoadAggregateFromBroker<ISeatMap>
	{
		ISeatMap LoadRootSeatMap();
	}
}