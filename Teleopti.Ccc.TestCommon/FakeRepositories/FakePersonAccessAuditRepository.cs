﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonAccessAuditRepository : IPersonAccessAuditRepository
	{
		public List<IPersonAccess> PersonAccesses = new List<IPersonAccess>();

		public void Add(IPersonAccess personAccess)
		{
			PersonAccesses.Add(personAccess);
		}

		public void Remove(IPersonAccess root)
		{
			throw new NotImplementedException();
		}

		public IPersonAccess Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IPersonAccess Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IPersonAccess> LoadAll()
		{
			return PersonAccesses;
		}

		public IEnumerable<IPersonAccess> LoadAudits(IPerson personId, DateTime startDate, DateTime endDate)
		{
			return PersonAccesses;
		}

		public void PurgeOldAudits(DateTime dateForPurging)
		{
			throw new NotImplementedException();
		}
	}
}