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
		[TestFixtureSetUp]
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
			var invokehandler = Container.Resolve<InvokeHandler>();

			invokehandler.Invoke(new TestJobParams("Dummy data",
			                                       "Name data"),
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
			var invokehandler = Container.Resolve<InvokeHandler>();

			Assert.IsNotNull(invokehandler);
		}
	}
}