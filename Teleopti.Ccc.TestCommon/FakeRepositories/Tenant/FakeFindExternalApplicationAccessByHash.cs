using System.Linq;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Tenant
{
	public class FakeFindExternalApplicationAccessByHash : IFindExternalApplicationAccessByHash
	{
		private readonly FakePersistExternalApplicationAccess _persister;

		public FakeFindExternalApplicationAccessByHash(FakePersistExternalApplicationAccess persister)
		{
			_persister = persister;
		}

		public ExternalApplicationAccess Find(string hash)
		{
			return _persister.Storage.FirstOrDefault(s => s.Hash == hash);
		}
	}
}