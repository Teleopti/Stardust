using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeCurrentUnitOfWorkFactory : ICurrentUnitOfWorkFactory
	{
		private readonly IUnitOfWorkFactory _current;

		public FakeCurrentUnitOfWorkFactory()
		{
		}
		public FakeCurrentUnitOfWorkFactory(IUnitOfWorkFactory current)
		{
			_current = current;
		}

		public IUnitOfWorkFactory Current()
		{
			return _current ?? new FakeUnitOfWorkFactory();
		}
	}
}