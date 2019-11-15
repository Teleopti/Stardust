using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using ManagerTest.Attributes;
using NUnit.Framework;
using SharpTestsEx;
using Stardust.Manager;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace ManagerTest
{
	[ManagerControllerValidationTest]
	[TestFixture]
	public class ManagerControllerValidationTest
	{
		public ManagerController ManagerController;
        public IJobRepository JobRepository;

        [Test]
		public void ShouldReturnOkWhenJobDoneReceived()
		{
			ManagerController.ControllerContext = new HttpControllerContext
			{
				Configuration = new HttpConfiguration()
			};
			ManagerController.Request = new HttpRequestMessage
			{
				RequestUri = new Uri("http://calabiro.com") 
			};
			var result = ManagerController.JobSucceed(Guid.NewGuid());
			var httpResponseMessage = result.ExecuteAsync(CancellationToken.None).Result;
			httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
			//Assert.IsInstanceOf(typeof(OkResult), result);
		}

		[Test]
		public void ShouldReturnBadRequestIfCancelThisJobGetsAnInvalidGuid()
		{
			var response = ManagerController.CancelJobByJobId(Guid.Empty);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);
		}

		[Test]
		public void ShouldReturnBadRequestIfHeartbeatGetsAnInvalidUri()
		{
			var response = ManagerController.WorkerNodeRegisterHeartbeat(null);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobFailedGetsANull()
		{
			var response = ManagerController.JobFailed(null);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobFailedGetsInvalidJobFailedModel()
		{
			var jobFailed = new JobFailed();

			var response = ManagerController.JobFailed(jobFailed);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobHistoryDetailsGetsAnInvalidGuid()
		{
			var response = ManagerController.GetJobDetailsByJobId(Guid.Empty);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobHistoryGetsAnInvalidGuid()
		{
			var response = ManagerController.GetJobByJobId(Guid.Empty);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);
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

			var response = ManagerController.AddItemToJobQueue(jobQueueItem);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);
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

			var response =
				ManagerController.AddItemToJobQueue(jobQueueItem);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);
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

			var response = ManagerController.AddItemToJobQueue(jobQueueItem);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);
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

			var response =
				ManagerController.AddItemToJobQueue(jobQueueItem);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);
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

			var response =
				ManagerController.AddItemToJobQueue(jobQueueItem);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);
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

			var response =
				ManagerController.AddItemToJobQueue(jobQueueItem);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);
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

			var response =
				ManagerController.AddItemToJobQueue(jobQueueItem);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);
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
				ManagerController.AddItemToJobQueue(jobQueueItem);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);
		}


		[Test]
		public void ShouldReturnBadRequestIJobDoneGetsAnInvalidUri()
		{
			var response = ManagerController.JobSucceed(Guid.Empty);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);
		}

        [Test]
        public void ShouldLogIfAggregateExceptionIsNull()
        {
            var jobFailed = new JobFailed
            {
                JobId = Guid.NewGuid(),
                AggregateException = null
            };
            ManagerController.Request = new HttpRequestMessage()
            {
                RequestUri = new Uri("http://calabiro.com")
            };

            var response = ManagerController.JobFailed(jobFailed);

            var jobDetails = JobRepository.GetJobDetailsByJobId(jobFailed.JobId);
            jobDetails.Single().Detail.Should().Be("No Exception specified for job");
        }
	}
}
