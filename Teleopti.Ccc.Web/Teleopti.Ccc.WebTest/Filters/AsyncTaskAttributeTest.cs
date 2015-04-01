using System;
using System.Globalization;
using System.Security.Principal;
using System.Threading;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Web.Filters;

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

		[Test]
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
		public void ShouldPutTaskInAsyncManagersParameters()
		{
			SetupDependencyResolver();
			var target = new AsyncTaskAttribute();
			var filterTester = new FilterTester();
			var controller = new TestTaskController(c => { });
			filterTester.UseController(controller);

			filterTester.InvokeFilter(target);

			Assert.That(() => controller.AsyncManager.Parameters.ContainsKey("task"), Is.True.After(1000, 10));
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

		[Test]
		public void ShouldCopyThreadCulture()
		{
			CultureInfo threadCulture = null;
			CultureInfo threadUICulture = null;
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-GB");
			Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("fi-FI");
			SetupDependencyResolver();
			var target = new AsyncTaskAttribute();
			var filterTester = new FilterTester();
			filterTester.UseController(new TestTaskController(c =>
			                                                  	{
																	threadCulture = Thread.CurrentThread.CurrentCulture;
																	threadUICulture = Thread.CurrentThread.CurrentUICulture;
																}));

			filterTester.InvokeFilter(target);

			Assert.That(() => threadCulture, Is.EqualTo(CultureInfo.GetCultureInfo("en-GB")).After(1000, 10));
			Assert.That(() => threadUICulture, Is.EqualTo(CultureInfo.GetCultureInfo("fi-FI")).After(1000, 10));
		}

		[Test]
		public void ShouldCopyThreadPrincipal()
		{
			// this is actually done automatically. the same does not apply for culture though.
			IPrincipal actual = null;
			var expected = new TestPrincipal();
			Thread.CurrentPrincipal = expected;
			SetupDependencyResolver();
			var target = new AsyncTaskAttribute();
			var filterTester = new FilterTester();
			filterTester.UseController(new TestTaskController(c =>
			                                                  	{
			                                                  		actual = Thread.CurrentPrincipal;
			                                                  	}));

			filterTester.InvokeFilter(target);

			Assert.That(() => actual, Is.SameAs(expected).After(1000, 10));
		}

		private class TestTaskController : AsyncController, FilterTester.ITestController
		{
			private readonly Action<TestTaskController> _taskAction;

			public TestTaskController(Action<TestTaskController> taskAction)
				: base() { _taskAction = taskAction; }

			public ActionResult DummyAction() { return null; }

			public void DummyActionTask() { _taskAction.Invoke(this); }

		}

		private class TestPrincipal : IPrincipal
		{
			public bool IsInRole(string role) { return true; }
			public IIdentity Identity { get { throw new NotImplementedException(); } }
		}
	}
}