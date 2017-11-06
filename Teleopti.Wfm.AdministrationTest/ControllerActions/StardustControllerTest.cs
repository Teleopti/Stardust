using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Infrastructure.Repositories.Stardust;
using Teleopti.Ccc.InfrastructureTest;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Core.Modules;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[DatabaseTest]
	public class StardustControllerTest : ISetup
	{
		public StardustController Target;
		public IStardustRepository StardustRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddModule(new WfmAdminModule());
		}

		[SetUp]
		public void Setup()
		{
			StardustRepositoryTestHelper.ClearQueue();
			StardustRepositoryTestHelper.ClearJobs();
		}

		[Test]
		public void JobQueueShouldFilterOnDataSource()
		{
			const string testTenant = "test Tenant";
			var testEvent = new UpdateStaffingLevelReadModelEvent{LogOnDatasource = testTenant};
			var testEventOtherTenant = new UpdateStaffingLevelReadModelEvent { LogOnDatasource = "Another tenant" };

			var job1 = new Job
			{
				JobId = Guid.NewGuid(),
				Serialized = JsonConvert.SerializeObject(testEvent),
				Type = "Type"
			};

			StardustRepositoryTestHelper.AddJobToQueue(job1);
			StardustRepositoryTestHelper.AddJobToQueue(new Job
			{
				JobId = Guid.NewGuid(),
				Serialized = JsonConvert.SerializeObject(testEventOtherTenant),
				Type = "Type"
			});
			var response = Target.JobQueueFiltered(1, 50, testTenant) as OkNegotiatedContentResult<IList<Job>>;
			response.Content.Count.Should().Be.EqualTo(1);
			response.Content.FirstOrDefault().JobId.Should().Be.EqualTo(job1.JobId);
		}

		[Test]
		public void JobShouldFilterOnDataSource()
		{
			const string testTenant = "test Tenant";
			var testEvent = new UpdateStaffingLevelReadModelEvent { LogOnDatasource = testTenant };
			var testEventOtherTenant = new UpdateStaffingLevelReadModelEvent { LogOnDatasource = "Another tenant" };

			var job1 = new Job
			{
				JobId = Guid.NewGuid(),
				Serialized = JsonConvert.SerializeObject(testEvent),
				Type = "Type"
			};

			StardustRepositoryTestHelper.AddJob(job1);
			StardustRepositoryTestHelper.AddJob(new Job
			{
				JobId = Guid.NewGuid(),
				Serialized = JsonConvert.SerializeObject(testEventOtherTenant),
				Type = "Type"
			});
			var response = Target.JobHistoryFiltered(1, 50, testTenant) as OkNegotiatedContentResult<IList<Job>>;
			response.Content.Count.Should().Be.EqualTo(1);
			response.Content.FirstOrDefault().JobId.Should().Be.EqualTo(job1.JobId);
		}

		[Test]
		public void JobShouldFilterOnType()
		{
			var testEvent = new UpdateStaffingLevelReadModelEvent();
			var anotherEvent = new ExportMultisiteSkillsToSkillEvent(); 

			var job1 = new Job
			{
				JobId = Guid.NewGuid(),
				Serialized = JsonConvert.SerializeObject(testEvent),
				Type = testEvent.GetType().ToString()
			};
			var job2 = new Job
			{
				JobId = Guid.NewGuid(),
				Serialized = JsonConvert.SerializeObject(anotherEvent),
				Type = anotherEvent.GetType().ToString()
			};

			StardustRepositoryTestHelper.AddJob(job1);
			StardustRepositoryTestHelper.AddJob(job2);

			var response = Target.JobHistoryFiltered(1, 50, null, "UpdateStaffingLevelReadModelEvent") as OkNegotiatedContentResult<IList<Job>>;
			response.Content.Count.Should().Be.EqualTo(1);
			response.Content.FirstOrDefault().JobId.Should().Be.EqualTo(job1.JobId);
		}

		[Test]
		public void JobQueueShouldFilterOnType()
		{
			var testEvent = new UpdateStaffingLevelReadModelEvent();
			var anotherEvent = new ExportMultisiteSkillsToSkillEvent();

			var job1 = new Job
			{
				JobId = Guid.NewGuid(),
				Serialized = JsonConvert.SerializeObject(testEvent),
				Type = testEvent.GetType().ToString()
			};
			var job2 = new Job
			{
				JobId = Guid.NewGuid(),
				Serialized = JsonConvert.SerializeObject(anotherEvent),
				Type = anotherEvent.GetType().ToString()
			};

			StardustRepositoryTestHelper.AddJobToQueue(job1);
			StardustRepositoryTestHelper.AddJobToQueue(job2);

			var response = Target.JobQueueFiltered(1, 50, null, "UpdateStaffingLevelReadModelEvent") as OkNegotiatedContentResult<IList<Job>>;
			response.Content.Count.Should().Be.EqualTo(1);
			response.Content.FirstOrDefault().JobId.Should().Be.EqualTo(job1.JobId);
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
 