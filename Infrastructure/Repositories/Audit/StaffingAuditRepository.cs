using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Infrastructure.Repositories.Audit
{
	public class StaffingAuditRepository : Repository<IStaffingAudit>, IStaffingAuditRepository
	{
		public StaffingAuditRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{
		}

		public IEnumerable<IStaffingAudit> LoadAudits(IPerson personId, DateTime startDate,
			DateTime endDate, string searchword = "")
		{
			var criteria = Session.CreateCriteria(typeof(StaffingAudit), "staffingAudit")
				.Add(Restrictions.Eq("ActionPerformedById", personId.Id.GetValueOrDefault()))
				.Add(Restrictions.Ge("TimeStamp", startDate))
				.Add(Restrictions.Le("TimeStamp", endDate));

			if (!string.IsNullOrEmpty(searchword))
				criteria = criteria
					.Add(Restrictions.Or(
						Restrictions.Like("ImportFileName", $"%{searchword}%"), 
						Restrictions.Like("BpoName", $"%{searchword}%")
					));

			var results = criteria.SetResultTransformer(Transformers.DistinctRootEntity)
			.List<IStaffingAudit>().ToList();
		
			return results;
		}

		public void PurgeOldAudits(DateTime daysBack)
		{
			Session.GetNamedQuery("PurgeStaffingAudit")
				.SetDateTime("DaysBack", daysBack).ExecuteUpdate();

		}
	}
}
