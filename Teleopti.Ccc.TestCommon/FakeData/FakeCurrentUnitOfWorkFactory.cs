using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeCurrentUnitOfWorkFactory : ICurrentUnitOfWorkFactory
	{
		public IUnitOfWorkFactory Current()
		{
			return new FakeUnitOfWorkFactory();
		}
	}
}