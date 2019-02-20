using System.Collections.Generic;
using NHibernate;
using NHibernate.Multi;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class AvailableDataRepository : Repository<IAvailableData>,IAvailableDataRepository
    {
        public AvailableDataRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
            : base(unitOfWork)
#pragma warning restore 618
        {
        }

	    public AvailableDataRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork, null, null)
	    {
	    }

        public virtual IList<IAvailableData> LoadAllAvailableData()
        {
            var q1 = Session.CreateCriteria(typeof(AvailableData))
                .Fetch("ApplicationRole")
                .SetResultTransformer(Transformers.DistinctRootEntity);
            var q2 = Session.CreateCriteria(typeof(AvailableData))
                .Fetch("AvailableSites");
            var q3 = Session.CreateCriteria(typeof(AvailableData))
                .Fetch("AvailableTeams");
            var q4 = Session.CreateCriteria(typeof(AvailableData))
                .Fetch("AvailablePersons");
            var q5 = Session.CreateCriteria(typeof(AvailableData))
                .Fetch("AvailableBusinessUnits");

            var res = Session.CreateQueryBatch()
                            .Add<AvailableData>(q1)
                            .Add<AvailableData>(q2)
                            .Add<AvailableData>(q3)
                            .Add<AvailableData>(q4)
                            .Add<AvailableData>(q5)
                            .GetResult<AvailableData>(0);

            return new List<IAvailableData>(CollectionHelper.ToDistinctGenericCollection<IAvailableData>(res));
        }
    }
}
