﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace Teleopti.Ccc.Intraday.TestApplication
{
	class Program
	{
		static void Main(string[] args)
		{
			string connectionString = ConfigurationManager.AppSettings["analyticsConnectionString"];
			IForecastProvider forecastProvider = new ForecastProvider(connectionString);
			IWorkloadQueuesProvider workloadQueuesProvider = new WorkloadQueuesProvider(connectionString);
			IDictionary<int, IList<QueueInterval>> queueDataDictionary = new Dictionary<int, IList<QueueInterval>>();
			IQueueDataPersister queueDataPersister = new QueueDataPersister(connectionString);
			TimeZoneprovider timeZoneprovider = new TimeZoneprovider(connectionString);
			var timeZoneIntervalLength = timeZoneprovider.Provide();
			var doReplace = false;

			Console.WriteLine("This tool will generate queue statistics for today for forecasted skills.");
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("CHECKLIST:");
			Console.WriteLine("1. Is there forecast data for today for your skills?");
			Console.WriteLine("");
			Console.WriteLine("2. Have you run ETL Nightly job for today?");
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("Checklist fulfilled? Press any key to continue.");
			Console.ReadKey();
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("Replace queue statistics for today? (Y/N)");
			ConsoleKeyInfo replace = Console.ReadKey();
			if (replace.KeyChar.ToString().ToUpper().Equals("Y"))
			{
				doReplace = true;
			}

			var time = IntervalHelper.GetValidIntervalTime(timeZoneIntervalLength.IntervalLength, DateTime.Now);
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("Stats will be generated up until {0}. Enter other time if needed.", time.ToShortTimeString());
			var timeText = Console.ReadLine();
			if (!string.IsNullOrEmpty(timeText))
			{
				if (!DateTime.TryParse(timeText, Thread.CurrentThread.CurrentCulture, DateTimeStyles.None, out time))
				{
					Console.WriteLine("{0} is not a valid time. Press any key to exit.", timeText);
					Console.ReadKey();
					return;
				}
				
				time = IntervalHelper.GetValidIntervalTime(timeZoneIntervalLength.IntervalLength, time);
            }
            Console.WriteLine("");
            Console.WriteLine("We're doing stuff. Please hang around...");

            var currentInterval = IntervalHelper.GetIntervalId(timeZoneIntervalLength.IntervalLength, time);


			var workloads = workloadQueuesProvider.Provide();

			foreach (var workloadInfo in workloads)
			{
				var targetQueue = getTargetQueue(workloadInfo.Queues, doReplace);
				if (targetQueue == null)
					continue;

				if (queueDataDictionary.ContainsKey(targetQueue.QueueId))
					continue;

				var forecastIntervals = forecastProvider.Provide(workloadInfo.WorkloadId, currentInterval);


				if (forecastIntervals.Count == 0 || Math.Abs(forecastIntervals.Sum(x => x.Calls)) < 0.001)
					continue;

				queueDataDictionary.Add(targetQueue.QueueId, generateQueueDataIntervals(forecastIntervals, targetQueue));
			}

			queueDataPersister.Persist(queueDataDictionary, doReplace);

			var skillsContainingQueue = new List<string>();

			foreach (var queueId in queueDataDictionary.Keys)
			{
				foreach (var workload in workloads)
				{
					foreach (var queue in workload.Queues)
					{
						if (queueId == queue.QueueId)
						{
							if (!skillsContainingQueue.Contains(workload.SkillName))
							{
								skillsContainingQueue.Add(workload.SkillName);
							}
							
						}
					}
				}
			}

			skillsContainingQueue.Sort();

			var skillsAffectedText = new StringBuilder();
			foreach (var skillName in skillsContainingQueue)
				skillsAffectedText.Append(skillName + " | ");

			Console.WriteLine("");
			Console.WriteLine("");
			if (skillsAffectedText.Length > 0)
			{
				Console.WriteLine("Queue stats were generated for the following skills:");
				Console.WriteLine(skillsAffectedText);
			}
			else
			{
				Console.WriteLine("No queue stats generated. Probably you did not fulfill the checklist.");
			}

			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("We're done! Press any key to exit.");
			Console.ReadKey();
		}

		private static IList<QueueInterval> generateQueueDataIntervals(IList<ForecastInterval> forecastIntervals, QueueInfo targetQueue)
		{
			var queueDataList = new List<QueueInterval>();
			foreach (var interval in forecastIntervals)
			{
				if (Math.Abs(interval.Calls) < 0.0001)
					continue;

				var algorithmProperties = getAlgorithmProperties(forecastIntervals);

				var index = forecastIntervals.IndexOf(interval);
				var calculatedHandleTime = (decimal) Math.Round(interval.HandleTime + algorithmProperties.HandleTimeConstant*index);
			    var talkTime = calculatedHandleTime * new decimal(0.9);
			    var acw = calculatedHandleTime * new decimal(0.1);
			    var offeredCalls = (decimal) Math.Round(
			        interval.Calls +
			        algorithmProperties.CallsConstant*index*(1 - ((double) index/algorithmProperties.IntervalCount)),
			        0);
			    var answeredCalls = Math.Round((offeredCalls * new Random().Next(75, 100)) / 100, 0);
			    var speedOfAnswer = createSpeedOfAnswer(answeredCalls);
			    var isASAWithinSL = speedOfAnswer / answeredCalls <= 20;
			    var answeredCallsWithinSL = isASAWithinSL
			        ? Math.Round(answeredCalls*new Random().Next(75, 100)/100, 0)
			        : Math.Round(answeredCalls*new Random().Next(50, 100)/100, 0);
                var queueData = new QueueInterval()
				{
					DateId = interval.DateId,
					IntervalId = interval.IntervalId,
					QueueId = targetQueue.QueueId,
					DatasourceId = targetQueue.DatasourceId,
					OfferedCalls = offeredCalls,
                    AnsweredCalls = answeredCalls,
                    AbandonedCalls = offeredCalls - answeredCalls,
                    TalkTime = talkTime,
					Acw = acw,
					HandleTime = (decimal)Math.Round(interval.HandleTime + algorithmProperties.HandleTimeConstant * index),
                    SpeedOfAnswer = speedOfAnswer,
                    AnsweredCallsWithinSL = answeredCallsWithinSL
                };
				queueDataList.Add(queueData);
			}

			return queueDataList;
		}

	    private static decimal createSpeedOfAnswer(decimal offeredCalls)
	    {
            var random = new Random().Next(5,28);
	        return offeredCalls * random;
	    }

	    private static QueueInfo getTargetQueue(IEnumerable<QueueInfo> queues, bool doReplace)
		{
			var queueInfos = queues as QueueInfo[] ?? queues.ToArray();
			if (!doReplace && queueInfos.Any(x => x.HasDataToday))
				return null;

			return queueInfos.First();
		}

		private static AlgorithmProperties getAlgorithmProperties(IList<ForecastInterval> forecastIntervals)
		{
			var algorithmProperties = new AlgorithmProperties();
			algorithmProperties.IntervalCount = forecastIntervals.Count;

			var midIntervalCalls = forecastIntervals[algorithmProperties.IntervalCount / 2].Calls;
			algorithmProperties.CallsConstant = midIntervalCalls / algorithmProperties.IntervalCount;

			var handleTimesAverage = forecastIntervals.Select(intervalData => intervalData.HandleTime).ToList().Average();
			algorithmProperties.HandleTimeConstant = handleTimesAverage * 0.03;

			return algorithmProperties;
		}
	}

	internal class AlgorithmProperties
	{
		public int IntervalCount { get; set; }
		public double CallsConstant { get; set; }
		public double HandleTimeConstant { get; set; }
	}
}
