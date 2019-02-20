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
		public FavoriteSearchRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork, null, null)
		{
		}

		public IEnumerable<IFavoriteSearch> FindAllForPerson(IPerson person, WfmArea area)
		{
			ICollection<IFavoriteSearch> retList = Session.CreateCriteria<FavoriteSearch>()
				.Add(Restrictions.Eq("Creator", person))
				.Add(Restrictions.Eq("WfmArea", area))
				.List<IFavoriteSearch>();

			return retList;
		}
	}


	public interface IFavoriteSearchRepository:IRepository<IFavoriteSearch>
	{
		IEnumerable<IFavoriteSearch> FindAllForPerson(IPerson person, WfmArea area);
	}
}