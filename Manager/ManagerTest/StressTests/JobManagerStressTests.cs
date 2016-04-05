using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net.Config;
using ManagerTest.Database;
using ManagerTest.Fakes;
using NUnit.Framework;
using Stardust.Manager;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace ManagerTest.StressTests
{
	[TestFixture, JobTests, Ignore]
	public class JobManagerStressTests : DatabaseTest
	{
		[TearDown]
		public void TearDown()
		{
			JobManager.Dispose();
		}

		public JobManager JobManager;
		public NodeManager NodeManager;
		public IHttpSender HttpSender;
		public IJobRepository JobRepository;
		public IWorkerNodeRepository WorkerNodeRepository;

		private readonly Uri _nodeUri1 = new Uri("http://localhost:9050/");
		private readonly Uri _nodeUri2 = new Uri("http://localhost:9051/");
		private readonly Uri _nodeUri3 = new Uri("http://localhost:9052/");
		private readonly Uri _nodeUri4 = new Uri("http://localhost:9053/");
		private readonly Uri _nodeUri5 = new Uri("http://localhost:9054/");

		private FakeHttpSender FakeHttpSender
		{
			get { return (FakeHttpSender) HttpSender; }
		}

		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
#if DEBUG
			var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));
#endif
		}

		[Test]
		public void StressTest1()
		{
			WorkerNodeRepository.AddWorkerNode(new WorkerNode
			{
				Id = Guid.NewGuid(),
				Url = _nodeUri1
			});

			WorkerNodeRepository.AddWorkerNode(new WorkerNode
			{
				Id = Guid.NewGuid(),
				Url = _nodeUri2
			});

			WorkerNodeRepository.AddWorkerNode(new WorkerNode
			{
				Id = Guid.NewGuid(),
				Url = _nodeUri3
			});

			WorkerNodeRepository.AddWorkerNode(new WorkerNode
			{
				Id = Guid.NewGuid(),
				Url = _nodeUri4
			});

			WorkerNodeRepository.AddWorkerNode(new WorkerNode
			{
				Id = Guid.NewGuid(),
				Url = _nodeUri5
			});

			var tasks = new List<Task<Guid>>();

			var random = new Random();

			for (var i = 0; i < 500; i++)
			{
				var rnd = random.Next(1, 6);

				switch (rnd)
				{
					case 1:
						tasks.Add(new Task<Guid>(() =>
						{
							var jobId = Guid.NewGuid();

							JobManager.AddJobDefinition(new JobDefinition
							{
								Id = jobId,
								Name = "Job Name Stress",
								UserName = "User Name Stress",
								Serialized = "Serialized Stress",
								AssignedNode = "",
								Status = "Added",
								Type = "Type"
							});

							return jobId;
						}));
						break;

					case 2:
						tasks.Add(new Task<Guid>(() =>
						{
							JobManager.CheckAndAssignNextJob();

							return Guid.Empty;
						}));
						break;

					case 3:
						tasks.Add(new Task<Guid>(() =>
						{
							JobManager.GetAllJobHistories();

							return Guid.Empty;
						}));

						break;

					case 4:
						tasks.Add(new Task<Guid>(() =>
						{
							JobManager.CancelJobByJobId(Guid.NewGuid());

							return Guid.Empty;
						}));

						break;

					case 5:
						tasks.Add(new Task<Guid>(() =>
						{
							JobManager.GetJobHistoryDetailsByJobId(Guid.NewGuid());

							return Guid.Empty;
						}));

						break;

					case 6:
						tasks.Add(new Task<Guid>(() =>
						{
							JobManager.CheckIfNodesAreAlive(TimeSpan.FromSeconds(5));

							return Guid.Empty;
						}));

						break;

				}
			}


			Parallel.ForEach(tasks, new ParallelOptions
			{
				MaxDegreeOfParallelism = 100
			}, task => task.Start());

			Task.WaitAll(tasks.ToArray());

			Assert.IsTrue(tasks.All(task => task.IsCompleted));
		}
	}
}