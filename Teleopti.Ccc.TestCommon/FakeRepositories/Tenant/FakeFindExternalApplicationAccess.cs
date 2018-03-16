using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Tenant
{
	public class FakeFindExternalApplicationAccess : IFindExternalApplicationAccess
	{
		private readonly FakePersistExternalApplicationAccess _persister;

		public FakeFindExternalApplicationAccess(FakePersistExternalApplicationAccess persister)
		{
			_persister = persister;
		}

		public ExternalApplicationAccess FindByTokenHash(string hash)
		{
			return _persister.Storage.FirstOrDefault(s => s.Hash == hash);
		}

		public IEnumerable<ExternalApplicationAccess> FindByPerson(Guid personId)
		{
			return _persister.Storage.Where(a => a.PersonId == personId);
		}
	}
}