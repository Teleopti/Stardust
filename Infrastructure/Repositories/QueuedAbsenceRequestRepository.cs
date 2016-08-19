using System.Collections.Generic;
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
	}
}