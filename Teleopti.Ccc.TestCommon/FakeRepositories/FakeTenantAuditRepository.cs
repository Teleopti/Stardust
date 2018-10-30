using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeTenantAuditRepository : ITenantAuditRepository
	{
		public List<ITenantAudit> TenantAuditList = new List<ITenantAudit>();

		public void Add(ITenantAudit root)
		{
			TenantAuditList.Add(root);
		}

		public void Remove(ITenantAudit root)
		{
			throw new NotImplementedException();
		}

		public ITenantAudit Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public ITenantAudit Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ITenantAudit> LoadAll()
		{
			return TenantAuditList;
		}

		public IEnumerable<ITenantAudit> LoadAudits(IPerson personId, DateTime startDate, DateTime endDate)
		{
			throw new NotImplementedException();
		}
	}
}