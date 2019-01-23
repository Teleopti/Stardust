using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

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

		public IList<IQueuedAbsenceRequest> FindByPersonRequestIds(IEnumerable<Guid> personRequestIds)
		{
			var returnQueuedAbsenceRequests = new List<IQueuedAbsenceRequest>();
			foreach (var idBatch in personRequestIds.Batch(1000))
			{
				returnQueuedAbsenceRequests.AddRange(Session.CreateCriteria(typeof(QueuedAbsenceRequest), "req")
					.Add(Restrictions.In("PersonRequest", idBatch.ToArray()))
					.List<IQueuedAbsenceRequest>());
			}
			return returnQueuedAbsenceRequests;
		}

		public void Send(List<Guid> queuedId, DateTime timeStamp)
		{
			timeStamp = timeStamp.Truncate(TimeSpan.FromSeconds(1));
			foreach (var batch in queuedId.Batch(1000))
			{
				var quesryString = "UPDATE[QueuedAbsenceRequest] SET Sent = :timestamp WHERE Id in (:ids)";
				var sqlQuery = Session.CreateSQLQuery(quesryString);
				sqlQuery.SetDateTime("timestamp", timeStamp);
				sqlQuery.SetParameterList("ids", batch);
				sqlQuery.ExecuteUpdate();
			}
		}
		
		public void Remove(DateTime sentTimeStamp)
		{
			if (sentTimeStamp.Year < 1800 || sentTimeStamp.Year > 9999) //bug 40907 sometimes looks like we get wrong timestamp
				return;
			var hql = @"DELETE FROM QueuedAbsenceRequest WHERE Sent = :sent";
			var sqlQuery = Session.CreateSQLQuery(hql);
			sqlQuery.SetDateTimeNoMs("sent", sentTimeStamp);
			sqlQuery.ExecuteUpdate();
		}

		public void Remove(List<Guid> ids)
		{
			var hql = @"DELETE FROM QueuedAbsenceRequest WHERE Id IN(:ids)";
			var sqlQuery = Session.CreateSQLQuery(hql);
			sqlQuery.SetParameterList("ids", ids);
			sqlQuery.ExecuteUpdate();
		}

		public int UpdateRequestPeriod(Guid id, DateTimePeriod period)
		{
			var hql = @"update [dbo].[QueuedAbsenceRequest] set StartDateTime = :startDateTime,
																EndDateTime = :endDateTime
						where [Sent] is null and PersonRequest = :id" ;
			var sqlQuery = Session.CreateSQLQuery(hql);
			sqlQuery.SetDateTime("startDateTime",period.StartDateTime);
			sqlQuery.SetDateTime("endDateTime", period.EndDateTime);
			sqlQuery.SetGuid("id", id);
			return sqlQuery.ExecuteUpdate();
		}

		public void ResetSent(DateTime eventSent)
		{
			var hql = @"update [dbo].[QueuedAbsenceRequest] set [Sent] = null where Sent = :sent";
			var sqlQuery = Session.CreateSQLQuery(hql);
			sqlQuery.SetDateTimeNoMs("sent", eventSent);
			sqlQuery.ExecuteUpdate();
		}

		public void ResetSent(List<Guid> ids)
		{
			var hql = @"update [dbo].[QueuedAbsenceRequest] set [Sent] = null where Id in(:ids)";
			var sqlQuery = Session.CreateSQLQuery(hql);
			sqlQuery.SetParameterList("ids", ids);
			sqlQuery.ExecuteUpdate();
		}

		public IList<IQueuedAbsenceRequest> FindByIds(IList<Guid> ids)
		{
			var returnQueuedAbsenceRequests = new List<IQueuedAbsenceRequest>();
			foreach (var idBatch in ids.Batch(1000))
			{
				returnQueuedAbsenceRequests.AddRange(Session.CreateCriteria(typeof(QueuedAbsenceRequest), "req")
					.Add(Restrictions.In("Id", idBatch.ToArray()))
					.List<IQueuedAbsenceRequest>());
			}
			return returnQueuedAbsenceRequests;
		}
	}
}