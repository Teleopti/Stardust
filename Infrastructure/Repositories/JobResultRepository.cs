using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for activities
    /// </summary>
    public class JobResultRepository : Repository<IJobResult>, IJobResultRepository
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public JobResultRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

		public JobResultRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
	    {
		    
	    }

		public ICollection<IJobResult> LoadHistoryWithPaging(PagingDetail pagingDetail, params string[] jobCategories)
		{
			var rowCount = Session.CreateCriteria<JobResult>()
				.Add(Restrictions.In("JobCategory", jobCategories))
				.SetProjection(Projections.RowCount())
				.FutureValue<int>();

			var result = Session.CreateCriteria<JobResult>()
				.Add(Restrictions.In("JobCategory", jobCategories))
				.AddOrder(Order.Desc("Timestamp"))
				.SetFirstResult(pagingDetail.Skip)
				.SetMaxResults(pagingDetail.Take)
				.List<IJobResult>();

			pagingDetail.TotalNumberOfResults = rowCount.Value;

			return result;
		}
	}
}