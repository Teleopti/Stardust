using Rhino.Mocks;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class DummyCurrentUnitOfWork : ICurrentUnitOfWork
	{
		public IUnitOfWork Current()
		{
			return MockRepository.GenerateStub<IUnitOfWork>();
		}
	}
}