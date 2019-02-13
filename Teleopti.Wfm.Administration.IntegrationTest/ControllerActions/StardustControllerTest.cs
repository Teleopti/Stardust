using System;
using System.Configuration;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Repositories.Stardust;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Wfm.Administration.Controllers;

namespace Teleopti.Wfm.Administration.IntegrationTest.ControllerActions
{
	[TestFixture]
	public class StardustControllerTest 
	{
		public StardustController Target;
		public IStardustRepository StardustRepository;

		[OneTimeSetUp]
		public void TestFixtureSetUp()
		{
			//TODO refactor to ioc
			StardustRepository = new StardustRepository(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);
			Target = new StardustController(StardustRepository, new FakeStardustSender(), new FakeTenants(),
				new StaffingSettingsReader49Days(), new FakePingNode(),null);
		}

		[Test]
		public void JobHistoryDetailsShouldNotCrash()
		{
			Target.JobHistoryDetails(Guid.NewGuid());
		}


		[Test]
		public void JobHistoryFilteredShouldNotCrash()
		{
			Target.JobHistoryFiltered(1,2);
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
		public void GetFailedJobsFilteredShouldNotCrash()
		{
			Target.FailedJobHistoryFiltered(1, 10);
		}
	}
}
 