using System;
using System.Collections;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public interface IPersonAccessAuditRepository : IRepository<IPersonAccess>
	{
		IEnumerable<IPersonAccess> LoadAudits(IPerson personId, DateTime startDate, DateTime endDate, string searchword = "");
		void PurgeOldAudits(DateTime dateForPurging);
	}
}