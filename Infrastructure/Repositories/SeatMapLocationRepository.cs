using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SeatMapLocationRepository : Repository<ISeatMapLocation>, ISeatMapLocationRepository
	{
		public SeatMapLocationRepository (ICurrentUnitOfWork currentUnitOfWork)
			: base (currentUnitOfWork, null, null)
		{
		}

		ISeatMapLocation ILoadAggregateByTypedId<ISeatMapLocation, Guid>.LoadAggregate (Guid id)
		{
			return LoadAggregate (id);
		}

		public ISeatMapLocation LoadAggregate (Guid id)
		{
			return Session.Query<ISeatMapLocation>()
				.FirstOrDefault (seatMapLoc => seatMapLoc.Id == id);
		}

		public ISeatMapLocation LoadRootSeatMap()
		{
			return Session.Query<ISeatMapLocation>().FirstOrDefault();
		}

		public IList<ISeatMapLocation> FindLocations (IList<Guid> locations)
		{
			return Session.Query<ISeatMapLocation>()
				.Where(seatMapLoc => locations.Contains(seatMapLoc.Id.Value)).ToList();
		}
	}
}
