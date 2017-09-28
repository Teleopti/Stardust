using System;
using System.Configuration;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Core.Stardust;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TestFixture]
	public class StardustControllerTest
	{
		public StardustController Target;
		public StardustRepository StardustRepository;

		[OneTimeSetUp]
		public void TestFixtureSetUp()
		{
			StardustRepository = new StardustRepository(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);
			Target = new StardustController(StardustRepository, new LegacyFakeEventPublisher(), new FakeTenants(), new StaffingSettingsReader());
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

		[Test]
		public void GetFailedJobsShouldNotCrash()
		{
			Target.FailedJobHistoryList(1, 10);
		}
	}
}