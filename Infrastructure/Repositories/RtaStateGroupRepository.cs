using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for state groups
    /// </summary>
    public class RtaStateGroupRepository : Repository<IRtaStateGroup>, IRtaStateGroupRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RtaStateGroupRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public RtaStateGroupRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public RtaStateGroupRepository(IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
        {
        }

	    public RtaStateGroupRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
	    {
	    }

	    /// <summary>
        /// Loads all complete graph.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-19
        /// </remarks>
        public IList<IRtaStateGroup> LoadAllCompleteGraph()
        {
            return Session.CreateCriteria(typeof (RtaStateGroup))
                .SetFetchMode("StateCollection",FetchMode.Join)
                .SetResultTransformer(Transformers.DistinctRootEntity)
                .List<IRtaStateGroup>();
        }
    }
}