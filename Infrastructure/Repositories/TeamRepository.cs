using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	/// <summary>
	/// Repository for teams
	/// </summary>
	public class TeamRepository : Repository<ITeam>, ITeamRepository
	{
		public static TeamRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new TeamRepository(currentUnitOfWork, null, null);
		}

		public static TeamRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new TeamRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}
		
		public TeamRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
		{

		}

		/// <summary>
		/// Finds all team by order.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: Dinesh Ranasinghe
		/// Created date: 2008-03-13
		/// </remarks>
		public ICollection<ITeam> FindAllTeamByDescription()
		{
			ISite site = null;
			var retList = Session.QueryOver<Team>()
				.JoinAlias(team => team.Site, () => site)
				.OrderBy(t => t.Description.Name)
				.Asc
				.ThenBy(t => site.Description.Name)
				.Asc
				.List<ITeam>();
			return retList;
		}

		public ICollection<ITeam> FindTeamByDescriptionName(string name)
		{
			ICollection<ITeam> retList = Session.CreateCriteria<Team>()
					   .Add(Restrictions.Eq("Description.Name", name))
					  .List<ITeam>();
			return retList;
		}

		public ICollection<ITeam> FindTeams(IEnumerable<Guid> teamId)
		{
			var result = new List<ITeam>();
			foreach (var teamBatch in teamId.Batch(200))
			{
				var currentBatchIds = teamBatch.ToArray();

				ICollection<ITeam> retList = Session.CreateCriteria<Team>()
					   .Add(Restrictions.InG("Id", currentBatchIds))
					   .Fetch("Site")
					   .SetResultTransformer(Transformers.DistinctRootEntity)
					   .List<ITeam>();

				result.AddRange(retList);
			}

			return result.GroupBy(r => r.Id).Select(r => r.First()).ToList();
		}

		public IEnumerable<ITeam> FindTeamsContain(string searchString, int maxHits)
		{
			if (maxHits < 1)
				return Enumerable.Empty<ITeam>();

			return Session.CreateCriteria<Team>()
				.Add(Restrictions.Like("Description.Name", searchString, MatchMode.Anywhere))
				.SetMaxResults(maxHits)
				.List<ITeam>();
		}

		public IEnumerable<ITeam> FindTeamsForSite(Guid siteId)
		{
			return Session.CreateCriteria<Team>()
				.Add(Restrictions.Eq("Site.Id", siteId))
				.AddOrder(Order.Asc("Description.Name"))
				.List<ITeam>();

		}
	}
}