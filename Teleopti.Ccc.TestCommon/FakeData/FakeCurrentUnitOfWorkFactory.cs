using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeCurrentUnitOfWorkFactory : ICurrentUnitOfWorkFactory
	{
		private IUnitOfWorkFactory _current;
		private readonly FakeStorage _storage;

		public FakeCurrentUnitOfWorkFactory(FakeStorage storage)
		{
			_storage = storage;
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