using System;
using System.Collections.Generic;
using Manager.Integration.Test.Models;
using Newtonsoft.Json;
using NodeTest.JobHandlers;
using FailingJobParams = Manager.Integration.Test.Params.FailingJobParams;
using TestJobParams = Manager.Integration.Test.Params.TestJobParams;

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

				for (var i = 1; i <= numberOfJobRequests; i++)
				{
					var failingJobParams = new FailingJobParams("Error message " + i);

					var failingJobParamsJson = JsonConvert.SerializeObject(failingJobParams);

					var job = new JobQueueItem
					{
						Name = "Job Name " + i,
						Serialized = failingJobParamsJson,
						Type = "NodeTest.JobHandlers.FailingJobParams",
						CreatedBy = "Test"
					};

					requestModels.Add(job);
				}
			}
			return requestModels;
		}

		public static List<JobQueueItem> GenerateTestJobRequests(int numberOfJobRequests,
		                                                              int duration)
		{
			List<JobQueueItem> jobQueueItems = new List<JobQueueItem>();

			if (numberOfJobRequests > 0)
			{
				for (var i = 1; i <= numberOfJobRequests; i++)
				{
					var testJobParams = new TestJobParams("Name " + i, duration);
					var testJobParamsToJson = JsonConvert.SerializeObject(testJobParams);

					var job = new JobQueueItem
					{
						Name = "Job Name " + i,
						Serialized = testJobParamsToJson,
						Type = "NodeTest.JobHandlers.TestJobParams",
						CreatedBy = "test"
					};
					jobQueueItems.Add(job);
				}
			}
			return jobQueueItems;
		}

		public static List<JobQueueItem> GenerateTestJobRequestsRandomDurationSomeFailing (int numberOfJobRequests)
		{
			List<JobQueueItem> jobQueueItems = new List<JobQueueItem>();

			if (numberOfJobRequests > 0)
			{
				var rand = new Random();
				for (var i = 1; i <= numberOfJobRequests; i++)
				{
					if (i % 10 != 0)
					{
						var testJobParams = new TestJobParams("Name " + i, rand.Next(0, 10));
						var testJobParamsToJson = JsonConvert.SerializeObject(testJobParams);

						var job = new JobQueueItem
						{
							Name = "Job Name " + i,
							Serialized = testJobParamsToJson,
							Type = "NodeTest.JobHandlers.TestJobParams",
							CreatedBy = "test"
						};
						jobQueueItems.Add(job);
					}
					else
					{
						var jobParams = new CrashingJobParams("error");
						var jsonJobParams = JsonConvert.SerializeObject(jobParams);

						var job = new JobQueueItem
						{
							Name = "Job Name " + i,
							Serialized = jsonJobParams,
							Type = "NodeTest.JobHandlers.CrashingJobParams",
							CreatedBy = "test"
						};
						jobQueueItems.Add(job);
					}
				}
			}
			return jobQueueItems;
		}
	}
}