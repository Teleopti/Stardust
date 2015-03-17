using System;
using System.Linq;
using NHibernate.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SeatMapLocationRepository : Repository<ISeatMapLocation>, ISeatMapLocationRepository
	{
		public SeatMapLocationRepository (IUnitOfWork unitOfWork)
			: base (unitOfWork)
		{
		}

		public SeatMapLocationRepository (IUnitOfWorkFactory unitOfWorkFactory)
			: base (unitOfWorkFactory)
		{
		}

		public SeatMapLocationRepository (ICurrentUnitOfWork currentUnitOfWork)
			: base (currentUnitOfWork)
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
		
		
	}
}
