using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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

		public IEnumerable<IFavoriteSearch> FindAllForPerson(Guid personId)
		{
			ICollection<IFavoriteSearch> retList = Session.CreateCriteria<FavoriteSearch>()
				.Add(Restrictions.Eq("Creator.Id", personId))
				.List<IFavoriteSearch>();

			return retList;
		}

		public IEnumerable<IFavoriteSearch> FindByPersonAndName(Guid personId, string name)
		{
			var searches =
				Session.CreateCriteria<FavoriteSearch>()
					.Add(Restrictions.Eq("Creator.Id", personId))
					.Add(Restrictions.Eq("Name", name)).List<IFavoriteSearch>();
			return searches;
		}
	}


	public interface IFavoriteSearchRepository:IRepository<IFavoriteSearch>
	{
		IEnumerable<IFavoriteSearch> FindAllForPerson(Guid personId);
		IEnumerable<IFavoriteSearch> FindByPersonAndName(Guid personId, string name);
	}
}