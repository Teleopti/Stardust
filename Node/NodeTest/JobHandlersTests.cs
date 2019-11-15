using System.Threading;
using Autofac;
using NodeTest.JobHandlers;
using NUnit.Framework;
using Stardust.Node.Interfaces;
using Stardust.Node.Workers;

namespace NodeTest
{
	[TestFixture]
	public class JobHandlersTests
	{
		[OneTimeSetUpAttribute]
		public void TestFixtureSetUp()
		{
			var builder = new ContainerBuilder();

			builder.RegisterInstance<IHandle<TestJobParams>>(new TestJobWorker(new TestJobCode()));

			builder.RegisterType<InvokeHandler>()
				.SingleInstance();

			Container = builder.Build();
		}

		private IContainer Container { get; set; }
		
		[Test]
		public void ShouldBeAbleToInvokeAHandler()
		{
			var invokeHandler = Container.Resolve<InvokeHandler>();

			var jobParams = new TestJobParams("Test Job", 1);

			invokeHandler.Invoke(jobParams,
			                     new CancellationTokenSource(),
			                     _=>{});
		}

		[Test]
		public void ShouldBeAbleToResolveAHandlerFromContainer()
		{
			var handler = Container.Resolve<IHandle<TestJobParams>>();

			Assert.IsNotNull(handler);
		}

		[Test]
		public void ShouldBeAbleToResolveInvokeHandlerFromContainer()
		{
			var invokeHandler = Container.Resolve<InvokeHandler>();

			Assert.IsNotNull(invokeHandler);
		}
	}
}