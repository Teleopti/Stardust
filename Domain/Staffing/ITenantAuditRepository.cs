using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public interface ITenantAuditRepository : IRepository<ITenantAudit>
	{
		IEnumerable<ITenantAudit> LoadAudits(IPerson personId, DateTime startDate, DateTime endDate);
	}
}