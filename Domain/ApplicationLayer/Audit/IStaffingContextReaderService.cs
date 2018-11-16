using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Audit
{
	public interface IStaffingContextReaderService
	{
		IEnumerable<AuditServiceModel> LoadAudits(IPerson personId, DateTime startDate, DateTime endDate);
		IEnumerable<AuditServiceModel> LoadAll();
	}
}