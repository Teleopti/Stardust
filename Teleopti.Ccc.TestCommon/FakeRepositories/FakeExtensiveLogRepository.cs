using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeExtensiveLogRepository : IExtensiveLogRepository
	{
		private readonly IList<ExtensiveLog> _extensiveLogs =new List<ExtensiveLog>();
		
		public void Add(object obj, Guid objId, string entityType)
		{
			_extensiveLogs.Add(new ExtensiveLog()
			{
				Id = objId
			});
		}

		public IList<ExtensiveLog> LoadAll()
		{
			return _extensiveLogs;
		}
		
	}
	public class FakeStaffingAuditRepository : IStaffingAuditRepository
	{
		public List<IStaffingAudit> StaffingAuditList = new List<IStaffingAudit>();
		
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
			throw new NotImplementedException();
		}
	}

	

	
}