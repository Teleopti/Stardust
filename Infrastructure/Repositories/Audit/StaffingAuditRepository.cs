using System;
using System.Collections;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
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
			//var results = new List<IStaffingAudit>();
			//results.AddRange(Session.CreateSQLQuery(
			//		@"Select [TimeStamp], ActionPerformedBy, [Action], Data, Area from Auditing.StaffingAudit
			//where TimeStamp between :StartDate and :EndDate and :PersonId = ActionPerformedBy")
			//	.SetDateTime("StartDate", startDate)
			//	.SetDateTime("EndDate", endDate)
			//	.SetGuid("PersonId",personId)
			//	.SetResultTransformer(Transformers.AliasToBean(typeof(StaffingAudit)))
			//	.SetReadOnly(true)
			//	.List<IStaffingAudit>());
			var results = new List<IStaffingAudit>();
			results.AddRange(Session.GetNamedQuery("StaffingAuditOnCriteria")
				.SetDateTime("StartDate", startDate)
				.SetDateTime("EndDate", endDate.AddDays(1).AddMinutes(-1))
				.SetEntity("PersonId", personId)
			//	.SetResultTransformer(Transformers.AliasToBean(typeof(StaffingAudit)))
			//	.SetReadOnly(true)
				.List<IStaffingAudit>());
			return results;
		}
	}
}
