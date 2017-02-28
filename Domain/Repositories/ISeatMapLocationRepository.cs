using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface ISeatMapLocationRepository : IRepository<ISeatMapLocation>, IWriteSideRepository<ISeatMapLocation>, ILoadAggregateFromBroker<ISeatMapLocation>
	{
		ISeatMapLocation LoadRootSeatMap();
		IList<ISeatMapLocation> FindLocations(IList<Guid> locations);
	}
}