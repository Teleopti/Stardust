using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer
{
	[TestFixture]
	public class SyncCommandDispatcherTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		public class TestCommand
		{
		}

	}
}