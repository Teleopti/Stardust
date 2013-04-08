using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for teams
    /// </summary>
    public class TeamRepository : Repository<ITeam>, ITeamRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The UnitOfWork.</param>
        public TeamRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public TeamRepository(IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
        {
        }

				public TeamRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
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
                      .SetResultTransformer(Transformers.DistinctRootEntity)
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
                       .SetFetchMode("Site", FetchMode.Join)
                      .SetResultTransformer(Transformers.DistinctRootEntity)
                      .List<ITeam>();

                result.AddRange(retList);
            }
            return result;
        }
    }
}