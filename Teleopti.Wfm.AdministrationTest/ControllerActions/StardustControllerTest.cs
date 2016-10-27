﻿using System;
using System.Configuration;
using NUnit.Framework;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Core.Stardust;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TestFixture]
	class StardustControllerTest
	{
		public StardustController Target;
		public StardustRepository StardustRepository;

		[OneTimeSetUp]
		public void TestFixtureSetUp()
		{
			StardustRepository = new StardustRepository(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);
			Target = new StardustController(StardustRepository);
		}

		[Test]
		public void RunningJobsListShouldNotCrash()
		{
			Target.RunningJobsList();
		}


		[Test]
		public void JobHistoryDetailsShouldNotCrash()
		{
			Target.JobHistoryDetails(Guid.NewGuid());
		}


		[Test]
		public void JobQueueListShouldNotCrash()
		{
			Target.JobQueueList(1, 10);
		}


		[Test]
		public void QueuedJobShouldNotCrash()
		{
			Target.QueuedJob(Guid.NewGuid());
		}


		[Test]
		public void DeleteQueuedJobsShouldNotCrash()
		{
			Target.DeleteQueuedJobs(new[] {Guid.NewGuid()});
		}
		

		[Test]
		public void JobHistoryListShouldNotCrash()
		{
			Target.JobHistoryList(1, 10);
		}

		[Test]
		public void JobHistoryListByNodeShouldNotCrash()
		{
			Target.JobHistoryList(Guid.NewGuid(), 1, 10);
		}

		[Test]
		public void WorkerNodesShouldNotCrash()
		{
			Target.WorkerNodes();
		}

		[Test]
		public void AliveWorkerNodesShouldNotCrash()
		{
			Target.AliveWorkerNodes();
		}

		[Test]
		public void WorkerNodesWithIdShouldNotCrash()
		{
			Target.WorkerNodes(Guid.NewGuid());
		}
	}
}