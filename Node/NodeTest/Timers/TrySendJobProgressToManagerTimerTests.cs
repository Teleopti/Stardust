using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NodeTest.Fakes;
using NUnit.Framework;
using Stardust.Node.Timers;
using Stardust.Node.Workers;

namespace NodeTest.Timers
{
	[TestFixture]
	public class TrySendJobProgressToManagerTimerTests
	{
		private NodeConfiguration _nodeConfiguration;
		private FakeHttpSender _httpSenderFake;

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


			_httpSenderFake = new FakeHttpSender();
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

		[Test]
		public void ShouldBeAbleToSend200ProgressesWith10ConcurrentProcesses()
		{
			const int numberOfProgesses = 200;
			const int numberOfConcurrentProcesses = 10;

			var timer =
				new TrySendJobProgressToManagerTimer(_nodeConfiguration,
				                                     _httpSenderFake,
				                                     1000);


			var numberOfProgressesReceived = 0;

			timer.SendJobProgressModelWithSuccessEventHandler += 
				(sender, model) => { numberOfProgressesReceived++; };


			List<Task<int>> tasks=new List<Task<int>>();

			for (var i = 0; i < numberOfConcurrentProcesses; i++)
			{
				tasks.Add(new Task<int>(() =>
				{
					var newGuid = Guid.NewGuid();

					for (var progressLoop = 0; progressLoop < numberOfProgesses; progressLoop++)
					{
						timer.SendProgress(newGuid, "Progress message nr : " + progressLoop);
					}

					return numberOfProgesses;
				}));
			}

			tasks.Add(new Task<int>(() =>
			{
				timer.Start();

				while (!timer.HasAllProgressesBeenSent())
				{

				}

				return 0;
			}));


			foreach (var task in tasks)
			{
				task.Start();
			}

			Task.WaitAll(tasks.ToArray());

			int totalNumberOfProgressesSent = tasks.Sum(task => task.Result);

			Assert.IsTrue(numberOfProgressesReceived == totalNumberOfProgressesSent,
			              "Number of progresses sent must be equal to received.");

			Assert.IsTrue(timer.HasAllProgressesBeenSent(),
			              "All progresses must be sent.");

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