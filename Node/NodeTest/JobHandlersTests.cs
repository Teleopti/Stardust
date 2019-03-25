using System;
using System.Threading;
using Autofac;
using Newtonsoft.Json;
using NodeTest.JobHandlers;
using NUnit.Framework;
using Stardust.Node.Entities;
using Stardust.Node.Interfaces;
using Stardust.Node.Workers;
using System.Collections.Generic;
using System.Linq;
using Stardust.Node.ReturnObjects;

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

			var jobParams = new TestJobParams("Test Job", 1);

			IEnumerable<object> returnObjects = null;

			invokehandler.Invoke(jobParams,
			                     new CancellationTokenSource(),
			                     _=>{},
								ref returnObjects
								);
		}

		[Test]
		public void ShouldBeAbleToInvokeAHandlerAndGetExitObjectInReturn()
		{
			var invokehandler = Container.Resolve<InvokeHandler>();

			var jobParams = new TestJobParams("Test Job", 1, true);

			IEnumerable<object> returnObjects = null;

			invokehandler.Invoke(jobParams,
				new CancellationTokenSource(),
				_ => { },
				ref returnObjects
			);

			Assert.IsNotNull(returnObjects);
			var list = ((List<object>) returnObjects);
			Assert.True(list.Count > 0);
			Assert.IsNotNull(list.FirstOrDefault(x => x is ExitApplication));
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