using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeStaffingAuditRepository : IStaffingAuditRepository
	{
		public List<IStaffingAudit> StaffingAuditList = new List<IStaffingAudit>();
		public int PurgeCounter { get; set; }

		public void Add(IStaffingAudit staffingAudit)
		{
			StaffingAuditList.Add(staffingAudit);
		}

		public void Remove(IStaffingAudit root)
		{
			throw new NotImplementedException();
		}

		public IStaffingAudit Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IStaffingAudit Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IStaffingAudit> LoadAll()
		{
			return StaffingAuditList;
		}

		public IEnumerable<IStaffingAudit> LoadAudits(IPerson personId, DateTime startDate,
			DateTime endDate)
		{
			return StaffingAuditList.Where(x => x.TimeStamp >= startDate && x.TimeStamp <= endDate.AddDays(1).AddMinutes(-1));
		}

		public void PurgeOldAudits(DateTime daysBack)
		{
			if(ThrowOnPurgeOldAudits) throw new Exception("ThrowOnPurgeOldAudits = true");
			PurgeCounter++;
		}

		public bool ThrowOnPurgeOldAudits { get; set; }
	}
}