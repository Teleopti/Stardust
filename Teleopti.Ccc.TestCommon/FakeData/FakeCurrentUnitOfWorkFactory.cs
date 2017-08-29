using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeCurrentUnitOfWorkFactory : ICurrentUnitOfWorkFactory
	{
		private IUnitOfWorkFactory _current;

		public ICurrentUnitOfWorkFactory WithCurrent(IUnitOfWorkFactory current)
		{
			_current = current;
			return this;
		}

		public IUnitOfWorkFactory Current()
		{
			return _current ?? new FakeUnitOfWorkFactory();
		}
	}
}