using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Filters
{
	[TestFixture]
	public class AsyncTaskAttributeTest
	{

		private static void SetupDependencyResolver()
		{
			var dependencyResolver = MockRepository.GenerateMock<IDependencyResolver>();
			DependencyResolver.SetResolver(dependencyResolver);
			dependencyResolver.Stub(x => x.GetServices(Arg<Type>.Is.Anything)).Return(new object[] { });
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldExecuteTaskMethod()
		{
			var taskInvokted = false;
			SetupDependencyResolver();
			var target = new AsyncTaskAttribute();
			var filterTester = new FilterTester();
			filterTester.UseController(new TestTaskController(c => { taskInvokted = true; }));

			filterTester.InvokeFilter(target);

			Assert.That(() => taskInvokted, Is.True.After(1000, 10));
		}

		[Test]
		public void ShouldPutExceptionInAsyncManagersParameters()
		{
			SetupDependencyResolver();
			var target = new AsyncTaskAttribute();
			var filterTester = new FilterTester();
			var controller = new TestTaskController(c => { throw new Exception(); });
			filterTester.UseController(controller);

			filterTester.InvokeFilter(target);

			Assert.That(() => controller.AsyncManager.Parameters.ContainsKey("exception"), Is.True.After(1000, 10));
		}

		[Test]
		public void ShouldIncrementAndDecremenetOutstandingOperations()
		{
			SetupDependencyResolver();
			var executionControlLock = new object();
			var target = new AsyncTaskAttribute();
			var filterTester = new FilterTester();
			var controller = new TestTaskController(c =>
			                                        	{
															Monitor.Enter(executionControlLock);
															Monitor.Exit(executionControlLock);
														});
			filterTester.UseController(controller);

			Assert.That(controller.AsyncManager.OutstandingOperations.Count, Is.EqualTo(0));
			Monitor.Enter(executionControlLock);
			filterTester.InvokeFilter(target);
			Assert.That(controller.AsyncManager.OutstandingOperations.Count, Is.EqualTo(1));
			Monitor.Exit(executionControlLock);
			Assert.That(() => controller.AsyncManager.OutstandingOperations.Count, Is.EqualTo(0).After(1000, 10));
		}

		private class TestTaskController : AsyncController, FilterTester.ITestController
		{
			private readonly Action<TestTaskController> _taskAction;

			public TestTaskController(Action<TestTaskController> taskAction)
				: base() { _taskAction = taskAction; }

			public ActionResult DummyAction() { return null; }

			public void DummyActionTask() { _taskAction.Invoke(this); }

		}
	}
}