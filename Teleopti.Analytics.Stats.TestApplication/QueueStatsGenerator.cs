using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Teleopti.Analytics.Stats.TestApplication
{
	internal class QueueStatsGenerator
	{
		public async Task CreateAsync(QueueDataParameters parameters)
		{
			using (var client = new HttpClient())
			{
				var webClient = new WebApiClient(client, parameters.NhibDataSourcename, parameters.QueueDataSourceId);
				int batchSize = int.Parse(ConfigurationManager.AppSettings["BatchSize"]);
				var modelsToPost = new List<QueueStatsModel>();
				int postCounter = 0;
				int expectedPostCount = (1440 / parameters.IntervalLength) * parameters.AmountOfDays * parameters.AmountOfQueues;
				var stopwatch = new Stopwatch();
				stopwatch.Start();
				int successful = 0, failed = 0;

				int intervalsPerDay = 1440 / parameters.IntervalLength;

				for (DateTime date = parameters.StartDate; date < (parameters.StartDate.AddDays(parameters.AmountOfDays)); date = date.AddDays(1))
				{
					for (int intervalId = 0; intervalId < intervalsPerDay; intervalId++)
					{
						for (int queue = 1; queue <= parameters.AmountOfQueues; queue++)
						{
							var model = new QueueStatsModel
							{
								IntervalStart = date.AddMinutes(intervalId * parameters.IntervalLength).ToString("yyyy-MM-dd HH:mm"),
								QueueId = "generatedQueue_" + queue,
								QueueName = "generatedQueue_" + queue,
								OfferedCalls = 100,
								AnsweredCalls = 50,
								AnsweredCallsWithinServiceLevel = 48,
								AbandonedCalls = 50,
								AbandonedCallsWithinServiceLevel = 2,
								AbandonedShortCalls = 0,
								OverflowOutCalls = 7,
								OverflowInCalls = 6,
								TalkTime = 120,
								AfterCallWork = 40,
								SpeedOfAnswer = 9,
								TimeToAbandon = 35,
								LongestDelayInQueueAnswered = 55,
								LongestDelayInQueueAbandoned = 60
							};

							modelsToPost.Add(model);
							postCounter++;

							if (modelsToPost.Count % batchSize == 0 || modelsToPost.Count == expectedPostCount || postCounter == expectedPostCount)
							{
								var result = await webClient.PostQueueDataAsync(modelsToPost);
								if (result) 
									successful += modelsToPost.Count;
								else 
									failed += modelsToPost.Count;

								modelsToPost.Clear();
								Console.Write("\rPosts successful: {0}; posts failed: {1}; Duration: {2}".PadLeft(Console.WindowWidth - Console.CursorLeft - 1), successful, failed, stopwatch.Elapsed);
							}
						}
					}

				stopwatch.Stop();
				Console.WriteLine("\nThe operation took: {0}", stopwatch.Elapsed);
				}

				stopwatch.Stop();
				Console.WriteLine("\nThe operation took: {0}", stopwatch.Elapsed);
			}

		}
	}
}