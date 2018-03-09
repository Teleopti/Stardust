using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Tenant
{
	public class FakePersistExternalApplicationAccess : IPersistExternalApplicationAccess
	{
		public List<ExternalApplicationAccess> Storage { get; } = new List<ExternalApplicationAccess>();

		public void Persist(ExternalApplicationAccess externalApplicationAccess)
		{
			Storage.Add(externalApplicationAccess);
		}
	}
}
