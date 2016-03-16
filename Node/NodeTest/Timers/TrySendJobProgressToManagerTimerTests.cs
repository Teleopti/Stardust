using System;
using System.Configuration;
using System.Reflection;
using System.Threading;
using NodeTest.Fakes;
using NUnit.Framework;
using Stardust.Node.API;
using Stardust.Node.Timers;

namespace NodeTest.Timers
{
	[TestFixture]
	public class TrySendJobProgressToManagerTimerTests
	{
		private NodeConfiguration _nodeConfiguration;
		private PostHttpRequestFake _httpSenderFake;

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			var baseAddress = new Uri(ConfigurationManager.AppSettings["BaseAddress"]);

			var managerLocation = new Uri(ConfigurationManager.AppSettings["ManagerLocation"]);

			var handlerAssembly = Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]);

			var nodeName = ConfigurationManager.AppSettings["NodeName"];

			_nodeConfiguration = new NodeConfiguration(baseAddress,
			                                           managerLocation,
			                                           handlerAssembly,
			                                           nodeName,
			                                           pingToManagerSeconds: 10);


			_httpSenderFake = new PostHttpRequestFake();
		}

		[Test]
		public void ShouldNotCrashWhenTimerStarts()
		{
			var timer =
				new TrySendJobProgressToManagerTimer(_nodeConfiguration,
													 _httpSenderFake,
													 1000);

			timer.SendProgress(Guid.NewGuid(), "Progress message");
			timer.SendProgress(Guid.NewGuid(), "Progress message");

			timer.Start();

			Thread.Sleep(TimeSpan.FromMinutes(5));	

			timer.Dispose();
		}

		[Test]
		public void ShouldBeAbleToClearAllJobProgresses()
		{
			var timer =
				new TrySendJobProgressToManagerTimer(_nodeConfiguration,
				                                     _httpSenderFake,
				                                     1000);

			timer.SendProgress(Guid.NewGuid(), "Progress message");
			timer.SendProgress(Guid.NewGuid(), "Progress message");

			Assert.IsTrue(timer.TotalNumberOfJobProgresses() == 2,
			              "Should have 2 job progresses.");

			timer.ClearAllJobProgresses();

			Assert.IsTrue(timer.TotalNumberOfJobProgresses() == 0,
						  "Should have 0 job progresses after clearing.");

			timer.Dispose();

		}

		[Test]
		public void ShouldBeAbleToInstantiateObject()
		{
			var timer =
				new TrySendJobProgressToManagerTimer(_nodeConfiguration,
				                                     _httpSenderFake,
				                                     1000);

			Assert.IsNotNull(timer);

			timer.Dispose();
		}

		[Test]
		public void ShouldHave100JobProgressesWhen100AreAdded()
		{
			var timer =
				new TrySendJobProgressToManagerTimer(_nodeConfiguration,
				                                     _httpSenderFake,
				                                     1000);

			Assert.IsNotNull(timer, "Should be able to instantiate timer.");

			for (var i = 0; i < 100; i++)
			{
				timer.SendProgress(Guid.NewGuid(), "Progress message");
			}

			Assert.IsTrue(timer.TotalNumberOfJobProgresses() == 100,
			              "100 job progresses are expected.");

			timer.Dispose();
		}

		[Test]
		public void ShouldHaveTwoJobProgressesWhenTwoWithSameGuidAreAdded()
		{
			var timer =
				new TrySendJobProgressToManagerTimer(_nodeConfiguration,
				                                     _httpSenderFake,
				                                     1000);

			Assert.IsNotNull(timer);

			var newGuid = Guid.NewGuid();

			timer.SendProgress(newGuid, "Progress message 1.");
			timer.SendProgress(newGuid, "Progress message 2.");

			timer.SendProgress(Guid.NewGuid(), "Progress message 3.");

			Assert.IsTrue(timer.TotalNumberOfJobProgresses(newGuid) == 2,
			              "Two job progresses with same GUID are expected.");

			timer.Dispose();
		}

		[Test, ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowArgumentNullExceptionWhenHttpSenderIsNull()
		{
			var timer =
				new TrySendJobProgressToManagerTimer(_nodeConfiguration,
				                                     null,
				                                     1000);

			timer.Dispose();
		}


		[Test, ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowArgumentNullExceptionWhenNodeConfigurationIsNull()
		{
			var timer =
				new TrySendJobProgressToManagerTimer(null,
				                                     null,
				                                     1000);

			timer.Dispose();
		}
	}
}