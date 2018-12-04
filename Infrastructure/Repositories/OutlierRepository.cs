using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// OutlierRepository class
    /// </summary>
    public class OutlierRepository : Repository<IOutlier>, IOutlierRepository
    {
        public OutlierRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
            : base(unitOfWork)
#pragma warning restore 618
        {
        }

				public OutlierRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {
		    
	    }

        /// <summary>
        /// Finds by workload. (including global)
        /// </summary>
        /// <param name="workload">The workload.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-15
        /// </remarks>
        public IList<IOutlier> FindByWorkload(IWorkload workload)
        {
            return Session.CreateCriteria(typeof(Outlier))
                .Add(Restrictions.Or(Restrictions.Eq("Workload",workload),Restrictions.IsNull("Workload")))
                .Fetch("Dates")
                .Fetch("OutlierDateProviders")
                .SetResultTransformer(Transformers.DistinctRootEntity)
                .List<IOutlier>();

        }
    }
}
