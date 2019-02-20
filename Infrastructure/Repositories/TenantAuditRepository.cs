using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class TenantAuditRepository : Repository<ITenantAudit>, ITenantAuditRepository
	{
		public TenantAuditRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork, null, null)
		{
		}

		public IEnumerable<ITenantAudit> LoadAudits(IPerson personId, DateTime startDate, DateTime endDate)
		{
			var results = new List<ITenantAudit>();
			results.AddRange(Session.GetNamedQuery("TenantAuditOnCriteria")
				.SetDateTime("StartDate", startDate)
				.SetDateTime("EndDate", endDate.AddDays(1).AddMinutes(-1))
				.SetEntity("PersonId", personId)
				.List<ITenantAudit>());
			return results;
		}
	}
}