using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class RtaStateGroupRepository : Repository<IRtaStateGroup>, IRtaStateGroupRepository
    {
        public RtaStateGroupRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
            : base(unitOfWork)
#pragma warning restore 618
        {
        }

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

	    public override bool ValidateUserLoggedOn
	    {
		    get { return false; }
	    }
    }
}