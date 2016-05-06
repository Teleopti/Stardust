using System;
using System.Collections.Generic;
using Manager.Integration.Test.Models;
using Manager.Integration.Test.TestParams;
using Newtonsoft.Json;

namespace Manager.Integration.Test.Helpers
{
	public static class JobHelper
	{

		public static TimeSpan GenerateTimeoutTimeInMinutes(int numberOfRequest,
		                                                    int latencyPerRequestInMinutes = 1)
		{
			if (numberOfRequest <= 0)
			{
				throw new ArgumentException("numberOfRequest");
			}

			return new TimeSpan(0, numberOfRequest*latencyPerRequestInMinutes, 0);
		}
		

		public static List<JobQueueItem> GenerateFailingJobParamsRequests(int numberOfJobRequests)
		{
			List<JobQueueItem> requestModels = null;

			if (numberOfJobRequests > 0)
			{
				requestModels = new List<JobQueueItem>();

				var loggedInUser = SecurityHelper.GetLoggedInUser();

				for (var i = 1; i <= numberOfJobRequests; i++)
				{
					var failingJobParams = new FailingJobParams("Error message " + i);

					var failingJobParamsJson = JsonConvert.SerializeObject(failingJobParams);

					var job = new JobQueueItem
					{
						Name = "Job Name " + i,
						Serialized = failingJobParamsJson,
						Type = "NodeTest.JobHandlers.FailingJobParams",
						CreatedBy = loggedInUser
					};

					requestModels.Add(job);
				}
			}
			return requestModels;
		}

		public static List<JobQueueItem> GenerateTestJobTimerRequests(int numberOfJobRequests,
		                                                              TimeSpan duration)
		{
			List<JobQueueItem> requestModels = null;

			if (numberOfJobRequests > 0)
			{
				requestModels = new List<JobQueueItem>();

				var loggedInUser = SecurityHelper.GetLoggedInUser();

				for (var i = 1; i <= numberOfJobRequests; i++)
				{
					var testJobTimerParams = new TestJobTimerParams("Name " + i,
					                                                duration);

					var testJobTimerParamsToJson = JsonConvert.SerializeObject(testJobTimerParams);

					var job = new JobQueueItem
					{
						Name = "Job Name " + i,
						Serialized = testJobTimerParamsToJson,
						Type = "NodeTest.JobHandlers.TestJobTimerParams",
						CreatedBy = loggedInUser
					};

					requestModels.Add(job);
				}
			}

			return requestModels;
		}
	}
}