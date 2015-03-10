using System;
using System.Linq;
using NHibernate.Linq;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SeatMapRepository : Repository<ISeatMap>, ISeatMapRepository
	{
		public SeatMapRepository (IUnitOfWork unitOfWork)
			: base (unitOfWork)
		{
		}

		public SeatMapRepository (IUnitOfWorkFactory unitOfWorkFactory)
			: base (unitOfWorkFactory)
		{
		}

		public SeatMapRepository (ICurrentUnitOfWork currentUnitOfWork)
			: base (currentUnitOfWork)
		{

		}

		ISeatMap ILoadAggregateByTypedId<ISeatMap, Guid>.LoadAggregate (Guid id)
		{
			return LoadAggregate (id);
		}

		public ISeatMap LoadAggregate (Guid id)
		{
			return Session.Query<ISeatMap>()
				.FirstOrDefault (seatMap => seatMap.Id == id);
		}

		public ISeatMap LoadRootSeatMap()
		{
			return Session.Query<ISeatMap>().FirstOrDefault();
		}
	}
}
