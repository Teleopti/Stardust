using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeCurrentUnitOfWorkFactory : ICurrentUnitOfWorkFactory
	{
		public IUnitOfWorkFactory Current()
		{
			return FakeUnitOfWorkFactory;
		}

		 public IUnitOfWorkFactory FakeUnitOfWorkFactory = new FakeUnitOfWorkFactory();
	}
}