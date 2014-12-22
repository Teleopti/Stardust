using System;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer
{
	[TestFixture]
	public class SyncCommandDispatcherTest
	{
		public IResolve ResolverWith(Type type, object instance)
		{
			var builder = new ContainerBuilder();
			builder.RegisterInstance(instance).As(type);
			return new AutofacResolve(builder.Build());
		}

		[Test]
		public void ShouldInvokeHandler()
		{
			var handler = new FakeHandler();
			var target = new SyncCommandDispatcher(ResolverWith(typeof(IHandleCommand<TestCommand>), handler));
			var command = new TestCommand();

			target.Execute(command);

			handler.CalledWithCommand.Should().Be(command);
		}

		public class TestCommand
		{
		}

		public class FakeHandler : IHandleCommand<TestCommand>
		{
			public object CalledWithCommand;

			public void Handle(TestCommand command)
			{
				CalledWithCommand = command;
			}
		}
	}
}