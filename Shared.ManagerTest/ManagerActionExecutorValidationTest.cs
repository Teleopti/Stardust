using System;
using System.Linq;
using ManagerTest.Attributes;
using NUnit.Framework;
using Shared.Stardust.Manager;
using SharpTestsEx;
using Stardust.Manager;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace ManagerTest
{
	[ManagerActionExecutorValidationTestAttribute]
	[TestFixture]
	public class ManagerActionExecutorValidationTest
	{
		public ManagerActionExecutor Target;
        public IJobRepository JobRepository;
        public const string Hostname = "AGreatHostname";

        [Test]
		public void ShouldReturnOkWhenJobDoneReceived()
		{
            var response = Target.JobSucceed(Guid.NewGuid(),Hostname);
            response.Result.Should().Be(SimpleResponse.Status.Ok);
        }

		[Test]
		public void ShouldReturnBadRequestIfCancelThisJobGetsAnInvalidGuid()
		{
			var response = Target.CancelJobByJobId(Guid.Empty, Hostname);
            response.Result.Should().Be(SimpleResponse.Status.BadRequest);
		}

		[Test]
		public void ShouldReturnBadRequestIfHeartbeatGetsAnInvalidUri()
		{
			var response = Target.WorkerNodeRegisterHeartbeat(null, Hostname);
            response.Result.Should().Be(SimpleResponse.Status.BadRequest);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobFailedGetsANull()
		{
			var response = Target.JobFailed(null, Hostname);
            response.Result.Should().Be(SimpleResponse.Status.BadRequest);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobFailedGetsInvalidJobFailedModel()
		{
			var jobFailed = new JobFailed();
            var response = Target.JobFailed(jobFailed, Hostname);
            response.Result.Should().Be(SimpleResponse.Status.BadRequest);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobHistoryDetailsGetsAnInvalidGuid()
		{
			var response = Target.GetJobDetailsByJobId(Guid.Empty);
            response.Result.Should().Be(SimpleResponse.Status.BadRequest);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobHistoryGetsAnInvalidGuid()
		{
			var response = Target.GetJobByJobId(Guid.Empty);
            response.Result.Should().Be(SimpleResponse.Status.BadRequest);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobQueueItemHasInvalidCreatedByValue()
		{
			var jobQueueItem = new JobQueueItem
			{
				Name = "Name",
				Serialized = "Serialized",
				Type = "Type",
				CreatedBy = string.Empty
			};

			var response = Target.AddItemToJobQueue(jobQueueItem, Hostname);
            response.Result.Should().Be(SimpleResponse.Status.BadRequest);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobRequestModelNameIsEmptyString()
		{
			var jobQueueItem = new JobQueueItem
			{
				Name = string.Empty,
				Serialized = "Serialized",
				Type = "Type",
				CreatedBy = "UserName"
			};

			var response = Target.AddItemToJobQueue(jobQueueItem, Hostname);
            response.Result.Should().Be(SimpleResponse.Status.BadRequest);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobRequestModelNameIsNull()
		{
			var jobQueueItem = new JobQueueItem
			{
				Name = null,
				Serialized = "Serialized",
				Type = "Type",
				CreatedBy = "UserName"
			};

			var response = Target.AddItemToJobQueue(jobQueueItem, Hostname);
            response.Result.Should().Be(SimpleResponse.Status.BadRequest);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobRequestModelSerializedIsEmptyString()
		{
			var jobQueueItem = new JobQueueItem
			{
				Name = "Name",
				Serialized = string.Empty,
				Type = "Type",
				CreatedBy = "UserName"
			};

			var response = Target.AddItemToJobQueue(jobQueueItem, Hostname);
            response.Result.Should().Be(SimpleResponse.Status.BadRequest);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobRequestModelSerializedIsNull()
		{
			var jobQueueItem = new JobQueueItem
			{
				Name = "Name",
				Serialized = null,
				Type = "Type",
				CreatedBy = "UserName"
			};

			var response = Target.AddItemToJobQueue(jobQueueItem, Hostname);
            response.Result.Should().Be(SimpleResponse.Status.BadRequest);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobRequestModelTypeIsEmptyString()
		{
			var jobQueueItem = new JobQueueItem
			{
				Name = "Name",
				Serialized = "Serialized",
				Type = string.Empty,
				CreatedBy = "UserName"
			};

			var response = Target.AddItemToJobQueue(jobQueueItem, Hostname);
            response.Result.Should().Be(SimpleResponse.Status.BadRequest);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobRequestModelTypeIsNull()
		{
			var jobQueueItem = new JobQueueItem
			{
				Name = "Name",
				Serialized = "Serialized",
				Type = null,
				CreatedBy = "UserName"
			};

			var response = Target.AddItemToJobQueue(jobQueueItem, Hostname);
            response.Result.Should().Be(SimpleResponse.Status.BadRequest);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobRequestModelUserNameIsNull()
		{
			var jobQueueItem = new JobQueueItem
			{
				Name = "Name",
				Serialized = "Serialized",
				Type = "Type",
				CreatedBy = null
			};

			var response =
				Target.AddItemToJobQueue(jobQueueItem, Hostname);

            response.Result.Should().Be(SimpleResponse.Status.BadRequest);
		}


		[Test]
		public void ShouldReturnBadRequestIJobDoneGetsAnInvalidUri()
		{
			var response = Target.JobSucceed(Guid.Empty, Hostname);

            response.Result.Should().Be(SimpleResponse.Status.BadRequest);
		}

        [Test]
        public void ShouldLogIfAggregateExceptionIsNull()
        {
            var jobFailed = new JobFailed
            {
                JobId = Guid.NewGuid(),
                AggregateException = null
            };
         

            var response = Target.JobFailed(jobFailed, Hostname);

            var jobDetails = JobRepository.GetJobDetailsByJobId(jobFailed.JobId);
            jobDetails.Single().Detail.Should().Be("No Exception specified for job");
        }
	}
}
