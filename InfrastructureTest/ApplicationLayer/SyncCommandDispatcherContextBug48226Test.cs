using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer
{
	[TestFixture]
	public class SyncCommandDispatcherContextBug48226Test
	{
		private static IResolve resolver()
		{
			var builder = new ContainerBuilder();
			builder.RegisterType<FakeHandler>().AsImplementedInterfaces();
			builder.RegisterType<TestDependency>().InstancePerLifetimeScope();
			return new AutofacResolve(builder.Build());
		}

		[Test]
		public void ShouldInvokeHandlerWithNewDependencyEachTime()
		{
			var res = resolver();
			var target = new SyncCommandDispatcher(res);
			var command = new TestCommand();

			target.Execute(command);

			((FakeHandler)res.Resolve(typeof(IHandleCommand<TestCommand>))).Dep.CallCount.Should().Be(0);
		}

		public class TestCommand
		{
		}

		public class TestDependency
		{
			public void Cache()
			{
				CallCount++;
			}

			public int CallCount { get; private set; }
		}

		public class FakeHandler : IHandleCommand<TestCommand>
		{
			public TestDependency Dep { get; }

			public FakeHandler(TestDependency dep)
			{
				Dep = dep;
			}

			public void Handle(TestCommand command)
			{
				Dep.Cache();
			}
		}
	}
}