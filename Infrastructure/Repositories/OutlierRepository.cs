using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// OutlierRepository class
    /// </summary>
    public class OutlierRepository : Repository<IOutlier>, IOutlierRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutlierRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public OutlierRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public OutlierRepository(IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
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
                .SetFetchMode("Dates", FetchMode.Join)
                .SetFetchMode("OutlierDateProviders", FetchMode.Join)
                .SetResultTransformer(Transformers.DistinctRootEntity)
                .List<IOutlier>();

        }
    }
}
