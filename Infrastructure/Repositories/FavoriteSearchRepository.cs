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
#pragma warning disable CS0618 // Type or member is obsolete
		public FavoriteSearchRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
#pragma warning restore CS0618 // Type or member is obsolete
		{
		}

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

		public IEnumerable<IFavoriteSearch> FindByPersonAndName(Guid personId, string name, WfmArea area)
		{
			var searches =
				Session.CreateCriteria<FavoriteSearch>()
					.Add(Restrictions.Eq("Creator.Id", personId))
					.Add(Restrictions.Eq("Name", name))
					.Add(Restrictions.Eq("WfmArea", (int)area))
					.List<IFavoriteSearch>();
			return searches;
		}
	}


	public interface IFavoriteSearchRepository:IRepository<IFavoriteSearch>
	{
		IEnumerable<IFavoriteSearch> FindAllForPerson(Guid personId, WfmArea area);
		IEnumerable<IFavoriteSearch> FindByPersonAndName(Guid personId, string name, WfmArea area);
	}
}