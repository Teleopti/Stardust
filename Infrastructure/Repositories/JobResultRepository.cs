using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for activities
    /// </summary>
    public class JobResultRepository : Repository<IJobResult>, IJobResultRepository
	{
		public JobResultRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork, null, null)
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

	    public void AddDetailAndCheckSuccess(Guid jobResultId, IJobResultDetail detail, int expectedSuccessful)
	    {
		    var jobResult = Session.Load<IJobResult>(jobResultId, LockMode.Upgrade);
			jobResult.AddDetail(detail);
			if (jobResult.Details.Count(x => x.DetailLevel == DetailLevel.Info && x.ExceptionMessage == null) >= expectedSuccessful)
				jobResult.FinishedOk = true;
		}

	    public IJobResult FindWithNoLock(Guid jobResultId)
	    {
			return Session.Load<IJobResult>(jobResultId, LockMode.None);
		}

		public IList<IJobResult> LoadAllWithNoLock()
		{
			return Session.CreateCriteria(typeof(IJobResult)).SetLockMode(LockMode.None).List<IJobResult>();
		}


	}
}