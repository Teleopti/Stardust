using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeCurrentUnitOfWorkFactory : ICurrentUnitOfWorkFactory
	{
		private readonly IUnitOfWorkFactory _current;
		public FakeCurrentUnitOfWorkFactory(IUnitOfWorkFactory current=null)
		{
			_current = current;
		}

		public IUnitOfWorkFactory Current()
		{
			return _current ?? new FakeUnitOfWorkFactory();
		}
	}
}