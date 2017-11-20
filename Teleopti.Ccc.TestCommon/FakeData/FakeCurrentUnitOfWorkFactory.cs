using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeCurrentUnitOfWorkFactory : ICurrentUnitOfWorkFactory
	{
		private IUnitOfWorkFactory _current;
		private readonly IFakeStorage _storage;

		public FakeCurrentUnitOfWorkFactory(IFakeStorage storage)
		{
			_storage = storage ?? new FakeStorageSimple();
		}

		public ICurrentUnitOfWorkFactory WithCurrent(IUnitOfWorkFactory current)
		{
			_current = current;
			return this;
		}

		public IUnitOfWorkFactory Current()
		{
			return _current ?? new FakeUnitOfWorkFactory(_storage, null, null, null);
		}
	}
}