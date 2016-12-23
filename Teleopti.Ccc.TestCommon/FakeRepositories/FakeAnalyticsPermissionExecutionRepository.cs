using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsPermissionExecutionRepository : IAnalyticsPermissionExecutionRepository
	{
		private class permissionExecutionEntry
		{
			public Guid PersonCode { get; set; }
			public int BusinessUnit { get; set; }
			public DateTime Timestamp { get; set; }
		}

		private readonly List<permissionExecutionEntry> entries = new List<permissionExecutionEntry>();
		private readonly INow _now;

		public FakeAnalyticsPermissionExecutionRepository(INow now)
		{
			_now = now;
		}

		public DateTime Get(Guid personId, int businessUnitId)
		{
			return entries.First(x => x.BusinessUnit == businessUnitId && x.PersonCode == personId).Timestamp;
		}

		public void Set(Guid personId, int businessUnitId)
		{
			entries.Add(new permissionExecutionEntry
			{
				PersonCode = personId,
				BusinessUnit = businessUnitId,
				Timestamp = _now.UtcDateTime()
			});
		}
	}
}