using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Audit
{
	public interface IPersonAccessContextReaderService
	{
		IEnumerable<AuditServiceModel> LoadAudits(IPerson personId, DateTime startDate, DateTime endDate, string searchword);
	}

	public enum PersonAuditActionResult
	{
		Change,
		NoChange,
		NotPermitted
	}

	public enum PersonAuditActionType
	{
		//dont remove the AuditTrail prefix because its used fo translation in the UI
		//Also don't remove the comments, they are used to 'fool' some translation code
		GrantRole,
		RevokeRole,
		AuditTrailSingleGrantRole, //Resources.AuditTrailSingleGrantRole
		AuditTrailSingleRevokeRole, //Resources.AuditTrailSingleRevokeRole
		AuditTrailMultiGrantRole, //Resources.AuditTrailMultiGrantRole
		AuditTrailMultiRevokeRole //Resources.AuditTrailMultiRevokeRole
	}
}