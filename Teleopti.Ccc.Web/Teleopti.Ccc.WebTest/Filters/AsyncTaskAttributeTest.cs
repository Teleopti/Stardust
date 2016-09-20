using System;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
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
			var taskInvoked = new ManualResetEventSlim(false);
			SetupDependencyResolver();
			var target = new AsyncTaskAttribute();
			var filterTester = new FilterTester();
			filterTester.UseController(new TestTaskController(c => taskInvoked.Set()));

			filterTester.InvokeFilter(target);
			var invoked = taskInvoked.Wait(TimeSpan.FromMinutes(1));

			invoked.Should().Be.True();
		}

		[Test]
		public void ShouldPutTaskInAsyncManagersParameters()
		{
			var taskInvoked = new ManualResetEventSlim(false);
			SetupDependencyResolver();
			var target = new AsyncTaskAttribute();
			var filterTester = new FilterTester();
			var controller = new TestTaskController(c => taskInvoked.Set());
			filterTester.UseController(controller);

			filterTester.InvokeFilter(target);
			var invoked = taskInvoked.Wait(TimeSpan.FromMinutes(1));

			invoked.Should().Be.True();
			controller.AsyncManager.Parameters.Any(x => x.Key == "task").Should().Be.True();
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
			var taskInvoked = new ManualResetEventSlim(false);
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
			                                                  		taskInvoked.Set();
			                                                  	}));

			filterTester.InvokeFilter(target);
			taskInvoked.Wait();

			threadCulture.Should().Be(CultureInfo.GetCultureInfo("en-GB"));
			threadUICulture.Should().Be(CultureInfo.GetCultureInfo("fi-FI"));
		}

		[Test]
		public void ShouldCopyThreadPrincipal()
		{
			var taskInvoked = new ManualResetEventSlim(false);
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
			                                                  		taskInvoked.Set();
			                                                  	}));

			filterTester.InvokeFilter(target);
			taskInvoked.Wait();

			actual.Should().Be(expected);
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