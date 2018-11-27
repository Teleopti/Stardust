using System;
using System.Collections.Generic;
using System.Linq;
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
			DateTime endDate)
		{
			var results = new List<IStaffingAudit>();
			results.AddRange(Session.GetNamedQuery("StaffingAuditOnCriteria")
				.SetDateTime("StartDate", startDate)
				.SetDateTime("EndDate", endDate.AddDays(1).AddMinutes(-1))
				.SetEntity("PersonId", personId)
				.List<IStaffingAudit>());
			//fix it in the SQL
			return results.Take(100);
		}

		public void PurgeOldAudits(DateTime daysBack)
		{
			Session.GetNamedQuery("PurgeStaffingAudit")
				.SetDateTime("DaysBack", daysBack).ExecuteUpdate();

		}
	}
}
