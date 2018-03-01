using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Configuration;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.RealTimeAdherence.Domain.Repositories
{
    public class RtaStateGroupRepository : Repository<IRtaStateGroup>, IRtaStateGroupRepository
    {
	    public RtaStateGroupRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
	    {
	    }

        public IList<IRtaStateGroup> LoadAllCompleteGraph()
        {
            return Session.CreateCriteria(typeof (RtaStateGroup))
                .SetFetchMode("StateCollection",FetchMode.Join)
                .SetResultTransformer(Transformers.DistinctRootEntity)
                .List<IRtaStateGroup>();
        }
    }
}