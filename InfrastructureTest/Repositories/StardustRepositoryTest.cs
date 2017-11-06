using System;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Infrastructure.Repositories.Stardust;
using Teleopti.Ccc.InfrastructureTest.Helper;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[DatabaseTest]
	public class StardustRepositoryTest
	{
		public IStardustRepository Target;

		[Test]
		public void JobQueueShouldFilterOnDataSource()
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

			StardustRepositoryTestHelper.AddJobToQueue(job1);
			StardustRepositoryTestHelper.AddJobToQueue(new Job
			{
				JobId = Guid.NewGuid(),
				Serialized = JsonConvert.SerializeObject(testEventOtherTenant),
				Type = "Type"
			});
			var queuedJobs = Target.GetAllQueuedJobs(new JobFilterModel {From = 1, To = 50, DataSource = testTenant});
			queuedJobs.Count.Should().Be.EqualTo(1);
			queuedJobs.Single().JobId.Should().Be.EqualTo(job1.JobId);
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
			var jobs = Target.GetAllJobs(new JobFilterModel {From = 1, To = 50, DataSource = testTenant});
			jobs.Count.Should().Be.EqualTo(1);
			jobs.Single().JobId.Should().Be.EqualTo(job1.JobId);
		}

		[Test]
		public void FailedJobShouldFilterOnDataSource()
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

			StardustRepositoryTestHelper.AddFailedJob(job1);
			StardustRepositoryTestHelper.AddFailedJob(new Job
			{
				JobId = Guid.NewGuid(),
				Serialized = JsonConvert.SerializeObject(testEventOtherTenant),
				Type = "Type"
			});
			StardustRepositoryTestHelper.AddJob(new Job
			{
				JobId = Guid.NewGuid(),
				Serialized = JsonConvert.SerializeObject(testEvent),
				Type = "Type"
			});
			var jobs = Target.GetAllFailedJobs(new JobFilterModel { From = 1, To = 50, DataSource = testTenant });
			jobs.Count.Should().Be.EqualTo(1);
			jobs.Single().JobId.Should().Be.EqualTo(job1.JobId);
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

			var jobs = Target.GetAllJobs(new JobFilterModel {From = 1, To = 50, Type = "UpdateStaffingLevelReadModelEvent"});
			jobs.Count.Should().Be.EqualTo(1);
			jobs.Single().JobId.Should().Be.EqualTo(job1.JobId);
		}

		[Test]
		public void FailedJobShouldFilterOnType()
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
			var job3 = new Job
			{
				JobId = Guid.NewGuid(),
				Serialized = JsonConvert.SerializeObject(testEvent),
				Type = testEvent.GetType().ToString()
			};

			StardustRepositoryTestHelper.AddFailedJob(job1);
			StardustRepositoryTestHelper.AddFailedJob(job2);
			StardustRepositoryTestHelper.AddJob(job3);

			var jobs = Target.GetAllFailedJobs(new JobFilterModel { From = 1, To = 50, Type = "UpdateStaffingLevelReadModelEvent" });
			jobs.Count.Should().Be.EqualTo(1);
			jobs.Single().JobId.Should().Be.EqualTo(job1.JobId);
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

			var queuedJobs = Target.GetAllQueuedJobs(new JobFilterModel {From = 1, To = 50, Type = "UpdateStaffingLevelReadModelEvent"});
			queuedJobs.Count.Should().Be.EqualTo(1);
			queuedJobs.Single().JobId.Should().Be.EqualTo(job1.JobId);
		}
	}
}
