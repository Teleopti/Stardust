using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PersonAccessAuditRepository : Repository<IPersonAccess>, IPersonAccessAuditRepository
	{
		public PersonAccessAuditRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{
		}

		public IEnumerable<IPersonAccess> LoadAudits(IPerson personId, DateTime startDate, DateTime endDate, string searchword)
		{
			var criteria = Session.CreateCriteria(typeof(PersonAccess), "personAccess")
				.Add(Restrictions.Eq("ActionPerformedById", personId.Id.GetValueOrDefault()))
				.Add(Restrictions.Ge("TimeStamp", new DateTime(startDate.Ticks,DateTimeKind.Utc)))
				.Add(Restrictions.Le("TimeStamp", new DateTime(endDate.AddDays(1).AddMinutes(-1).Ticks,DateTimeKind.Utc)));

			if (!string.IsNullOrEmpty(searchword))
				criteria = criteria
					.Add(Restrictions.Or(
						Restrictions.Like("SearchKeys", $"%{searchword}%"),
						Restrictions.Like("ActionPerformedOn", $"%{searchword}%")));

			criteria.AddOrder(Order.Desc("TimeStamp")).SetMaxResults(100);

			var results = criteria.SetResultTransformer(Transformers.DistinctRootEntity)
				.List<IPersonAccess>().ToList();
			return results;
		}

		public void PurgeOldAudits(DateTime dateForPurging)
		{
			Session.GetNamedQuery("PurgePersonAccess")
				.SetDateTime("DaysBack", dateForPurging).ExecuteUpdate();
		}
	}
}
