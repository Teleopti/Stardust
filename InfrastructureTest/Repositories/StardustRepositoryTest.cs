using System;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Infrastructure.Events;
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
		public void JobQueueShouldShowtopXOfFiltered()
		{
			const string testTenant = "test Tenant";
			var testEvent = new UpdateStaffingLevelReadModelEvent { LogOnDatasource = testTenant };
			var testEventOtherTenant = new UpdateStaffingLevelReadModelEvent { LogOnDatasource = "Another tenant" };

			StardustRepositoryTestHelper.AddJobToQueue(new Job
			{
				JobId = Guid.NewGuid(),
				Serialized = JsonConvert.SerializeObject(testEvent),
				Type = "Type"
			});
			Thread.Sleep(1000); //make sure there is different timestamps

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
			Thread.Sleep(1000); //make sure there is different timestamps

			var job2 = new Job
			{
				JobId = Guid.NewGuid(),
				Serialized = JsonConvert.SerializeObject(testEvent),
				Type = "Type"
			};

			StardustRepositoryTestHelper.AddJobToQueue(job2);

			var queuedJobs = Target.GetAllQueuedJobs(new JobFilterModel { From = 1, To = 2, DataSource = testTenant });
			queuedJobs.Count.Should().Be.EqualTo(2);
			queuedJobs.First().JobId.Should().Be.EqualTo(job2.JobId);
			queuedJobs.Second().JobId.Should().Be.EqualTo(job1.JobId);
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
		public void JobFilterShouldShowTopXOfItsDataSource()
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
			var job2 = new Job
			{
				JobId = Guid.NewGuid(),
				Serialized = JsonConvert.SerializeObject(testEvent),
				Type = "Type"
			};

			StardustRepositoryTestHelper.AddJob(new Job
			{
				JobId = Guid.NewGuid(),
				Serialized = JsonConvert.SerializeObject(testEvent),
				Type = "Type"
			});
			Thread.Sleep(1000); //make sure there is different timestamps
			StardustRepositoryTestHelper.AddJob(job1);
			StardustRepositoryTestHelper.AddJob(new Job
			{
				JobId = Guid.NewGuid(),
				Serialized = JsonConvert.SerializeObject(testEventOtherTenant),
				Type = "Type"
			});
			Thread.Sleep(1000); //make sure there is different timestamps
			StardustRepositoryTestHelper.AddJob(job2);
			

			var jobs = Target.GetAllJobs(new JobFilterModel { From = 1, To = 2, DataSource = testTenant });
			jobs.Count.Should().Be.EqualTo(2);
			jobs.First().JobId.Should().Be.EqualTo(job2.JobId);
			jobs.Second().JobId.Should().Be.EqualTo(job1.JobId);
		}

		[Test]
		public void JobFilterShouldOnlyShowFilteredDays()
		{
			const string testTenant = "test Tenant";
			var testEvent = new UpdateStaffingLevelReadModelEvent { LogOnDatasource = testTenant };

			var job1 = new Job
			{
				JobId = Guid.NewGuid(),
				Serialized = JsonConvert.SerializeObject(testEvent),
				Type = "Type",
				Started = new DateTime(2017, 01, 01, 10, 0, 0),
				Ended = new DateTime(2017, 01, 01, 11, 0, 0)
			};
			var job2 = new Job
			{
				JobId = Guid.NewGuid(),
				Serialized = JsonConvert.SerializeObject(testEvent),
				Type = "Type",
				Started = new DateTime(2017, 01, 03, 10, 0, 0),
				Ended = new DateTime(2017, 01, 03, 11, 0, 0)
			};
			StardustRepositoryTestHelper.AddJob(job1);
			StardustRepositoryTestHelper.AddJob(job2);


			var jobs = Target.GetAllJobs(new JobFilterModel { From = 1, To = 2, FromDate = new DateTime(2017,01,01), ToDate = new DateTime(2017,01,01)});
			jobs.Count.Should().Be.EqualTo(1);
			jobs.Single().JobId.Should().Be.EqualTo(job1.JobId);
		}

		[Test]
		public void FailedJobFilterShouldShowTopXOfItsDataSource()
		{
			const string testTenant = "test Tenant";
			var testEvent = new UpdateStaffingLevelReadModelEvent { LogOnDatasource = testTenant };
			var testEventOtherTenant = new UpdateStaffingLevelReadModelEvent { LogOnDatasource = "Another tenant" };

			StardustRepositoryTestHelper.AddFailedJob(new Job
			{
				JobId = Guid.NewGuid(),
				Serialized = JsonConvert.SerializeObject(testEvent),
				Type = "Type"
			});
			Thread.Sleep(1000); //make sure there is different timestamps
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
			Thread.Sleep(1000); //make sure there is different timestamps
			StardustRepositoryTestHelper.AddJob(new Job
			{
				JobId = Guid.NewGuid(),
				Serialized = JsonConvert.SerializeObject(testEvent),
				Type = "Type"
			});

			var job2 = new Job
			{
				JobId = Guid.NewGuid(),
				Serialized = JsonConvert.SerializeObject(testEvent),
				Type = "Type"
			};

			StardustRepositoryTestHelper.AddFailedJob(job2);

			var jobs = Target.GetAllFailedJobs(new JobFilterModel { From = 1, To = 2, DataSource = testTenant });
			jobs.Count.Should().Be.EqualTo(2);
			jobs.First().JobId.Should().Be.EqualTo(job2.JobId);
			jobs.Second().JobId.Should().Be.EqualTo(job1.JobId);
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

		[Test]
		public void ShouldCutOfNamespaceFromType()
		{
			var testEvent = new UpdateStaffingLevelReadModelEvent();
			var job1 = new Job
			{
				JobId = Guid.NewGuid(),
				Serialized = JsonConvert.SerializeObject(testEvent),
				Type = testEvent.GetType().ToString()
			};
			StardustRepositoryTestHelper.AddJob(job1);
			var types = Target.GetAllTypes();
			types.Single().Should().Be.EqualTo("UpdateStaffingLevelReadModelEvent");
		}

		[Test]
		public void ShouldNotManipulateStringsWithoutNameSpace()
		{
			var testEvent = new IndexMaintenanceEvent();
			var job1 = new Job
			{
				JobId = Guid.NewGuid(),
				Serialized = JsonConvert.SerializeObject(testEvent),
				Type = testEvent.GetType().ToString()
			};
			StardustRepositoryTestHelper.AddJob(job1);
			var types = Target.GetAllTypes();
			types.Single().Should().Be.EqualTo("IndexMaintenanceEvent");
		}
	}
}
