using System;
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
				var webClient = new WebApiClient(client);
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
								NhibName = parameters.NhibDataSourcename,
								LogObjectName = parameters.QueueDataSourceName,
								DateAndTimeString = date.AddMinutes(intervalId * parameters.IntervalLength).ToString("yyyy-MM-dd HH:mm"),
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


							var result = await webClient.PostQueueDataAsync(model);
							if (result) successful++;
							else failed++;

							Console.Write("\rNumber of successful posts: {0}; Number of failed posts: {1}                ", successful, failed);
						}
					}
				}

				stopwatch.Stop();
				Console.WriteLine("\nThe operation took: {0}", stopwatch.Elapsed);
			}
			
		}
	}
}