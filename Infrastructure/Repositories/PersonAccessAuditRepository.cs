using System;
using System.Collections.Generic;
using System.Linq;
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

		public IEnumerable<IPersonAccess> LoadAudits(IPerson personId, DateTime startDate, DateTime endDate)
		{
			var results = new List<IPersonAccess>();
			results.AddRange(Session.GetNamedQuery("PersonAccessAuditOnCriteria")
				.SetDateTime("StartDate", startDate)
				.SetDateTime("EndDate", endDate.AddDays(1).AddMinutes(-1))
				.SetEntity("PersonId", personId)
				.List<IPersonAccess>());
			//do it in SQL
			return results.Take(100);
		}

		public void PurgeOldAudits(DateTime dateForPurging)
		{
			Session.GetNamedQuery("PurgePersonAccess")
				.SetDateTime("DaysBack", dateForPurging).ExecuteUpdate();
		}
	}
}
