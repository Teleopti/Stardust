using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer
{
	[TestFixture]
	public class SyncCommandDispatcherTest
	{
		[Test]
		public void ShouldInvokeHandler()
		{
			var handler = MockRepository.GenerateMock<IHandleCommand<TestCommand>>();
			var resolver = MockRepository.GenerateMock<IResolve>();
			resolver.Stub(x => x.Resolve(typeof(IHandleCommand<TestCommand>))).Return(handler);
			var target = new SyncCommandDispatcher(resolver);
			var command = new TestCommand();

			target.Execute(command);

			handler.AssertWasCalled(x => x.Handle(command));
		}

		public class TestCommand
		{
		}

	}
}