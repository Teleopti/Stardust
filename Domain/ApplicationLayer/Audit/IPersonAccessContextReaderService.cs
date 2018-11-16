using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Audit
{
	public interface IPersonAccessContextReaderService
	{
		IEnumerable<AuditServiceModel> LoadAll();
		IEnumerable<AuditServiceModel> LoadAudits(IPerson personId, DateTime startDate, DateTime endDate);
	}

	public enum PersonAuditActionResult
	{
		Change,
		NoChange,
		NotPermitted
	}

	public enum PersonAuditActionType
	{
		GrantRole,
		RevokeRole,
		SingleGrantRole,
		SingleRevokeRole,
		MultiGrantRole,
		MultiRevokeRole
	}
}