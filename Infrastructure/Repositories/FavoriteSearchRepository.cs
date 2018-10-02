using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class FavoriteSearchRepository : Repository<IFavoriteSearch>, IFavoriteSearchRepository
	{
		public FavoriteSearchRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}

		public IEnumerable<IFavoriteSearch> FindAllForPerson(Guid personId, WfmArea area)
		{
			ICollection<IFavoriteSearch> retList = Session.CreateCriteria<FavoriteSearch>()
				.Add(Restrictions.Eq("Creator.Id", personId))
				.Add(Restrictions.Eq("WfmArea", (int)area))
				.List<IFavoriteSearch>();

			return retList;
		}
	}


	public interface IFavoriteSearchRepository:IRepository<IFavoriteSearch>
	{
		IEnumerable<IFavoriteSearch> FindAllForPerson(Guid personId, WfmArea area);
	}
}