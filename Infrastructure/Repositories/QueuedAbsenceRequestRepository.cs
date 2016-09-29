using System;
using System.Collections.Generic;
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

		public void Send(List<Guid> requestId, DateTime timeStamp)
		{
			var quesryString = "UPDATE[QueuedAbsenceRequest] SET Sent = :timestamp WHERE PersonRequest in (:ids)";
			var sqlQuery = Session.CreateSQLQuery(quesryString);
			sqlQuery.SetDateTime("timestamp", timeStamp);
			sqlQuery.SetParameterList("ids", requestId);
			sqlQuery.ExecuteUpdate();
		}


		public void Remove(DateTime sentTimeStamp)
		{
			if (sentTimeStamp.Year < 1800 || sentTimeStamp.Year > 9999) //bug 40907 sometimes looks like we get wrong timestamp
				return;
			var hql = @"DELETE FROM QueuedAbsenceRequest WHERE Sent = :sent";
			var sqlQuery = Session.CreateSQLQuery(hql);
			sqlQuery.SetDateTime("sent", sentTimeStamp);
			sqlQuery.ExecuteUpdate();
		}

		public void CheckAndUpdateSent(int minutes)
		{
			var hql = @"update [dbo].[QueuedAbsenceRequest] set [Sent] = null where [Sent] < :timeStamp";
			var sqlQuery = Session.CreateSQLQuery(hql);
			sqlQuery.SetDateTime("timeStamp", DateTime.UtcNow.AddMinutes(-minutes));
			sqlQuery.ExecuteUpdate();
		}

	}
}