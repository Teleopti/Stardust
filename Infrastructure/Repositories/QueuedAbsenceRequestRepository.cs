using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class QueuedAbsenceRequestRepository : Repository<IQueuedAbsenceRequest>, IQueuedAbsenceRequestRepository
	{
		public QueuedAbsenceRequestRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{

		}

		public IList<IQueuedAbsenceRequest> Find(DateTimePeriod period)
		{
			var startCriteria = Restrictions.And(Restrictions.Le("StartDateTime", period.EndDateTime),
				Restrictions.Ge("EndDateTime", period.StartDateTime));
			var endCriteria = Restrictions.And(Restrictions.Ge("EndDateTime", period.StartDateTime),
				Restrictions.Le("StartDateTime", period.EndDateTime));

			return Session.CreateCriteria(typeof(QueuedAbsenceRequest))
				.Add(Restrictions.Or(startCriteria, endCriteria))
				.AddOrder(Order.Asc("Created"))
				.List<IQueuedAbsenceRequest>();
		}

		public void Send(List<Guid> requestIds, DateTime timeStamp)
		{
			var quesryString = "UPDATE[QueuedAbsenceRequest] SET Sent = :timestamp WHERE PersonRequest in (:ids)";
			var sqlQuery = Session.CreateSQLQuery(quesryString);
			sqlQuery.SetDateTime("timestamp", timeStamp);
			sqlQuery.SetParameterList("ids", requestIds);
			sqlQuery.ExecuteUpdate();
		}


		public void Remove(IEnumerable<Guid> absenceRequests)
		{
			var hql = @"DELETE FROM QueuedAbsenceRequest WHERE personRequest in (:absenceRequests)";
			var sqlQuery = Session.CreateSQLQuery(hql);
			sqlQuery.SetParameterList("absenceRequests", absenceRequests.ToArray());
			sqlQuery.ExecuteUpdate();
		}
	}
}