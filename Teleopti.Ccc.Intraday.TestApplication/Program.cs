using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.Intraday.TestApplication
{
	class Program
	{
		static void Main(string[] args)
		{
			var doReplace = false;
			Console.WriteLine("This tool will generate queue statistics for today for forecasted skills.");
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("CHECKLIST:");
			Console.WriteLine("1. Make sure you have forecast data for today for the skills you want to monitor in Intraday.");
			Console.WriteLine("");
			Console.WriteLine("2. Run ETL Nightly job for today.");
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("When the above checklist is fulfilled, press any key to continue.");
			Console.ReadKey();
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("Would you like to replace potential existing queue statistics for today? (Y/N)");
			var replace = Console.ReadLine();
			if (replace.ToUpper().Equals("Y"))
			{
				doReplace = true;
			}

			string connectionString = ConfigurationManager.AppSettings["analyticsConnectionString"];
			IForecastProvider forecastProvider = new ForecastProvider(connectionString);
			IWorkloadQueuesProvider workloadQueuesProvider = new WorkloadQueuesProvider(connectionString);
			IDictionary<int, IList<QueueInterval>> queueDataDictionary = new Dictionary<int, IList<QueueInterval>>();
			IQueueDataPersister queueDataPersister = new QueueDataPersister(connectionString);

			var workloads = workloadQueuesProvider.Provide();

			foreach (var workloadInfo in workloads)
			{
				var targetQueue = getTargetQueue(workloadInfo.Queues, doReplace);
				if (targetQueue == null)
					continue;

				if (queueDataDictionary.ContainsKey(targetQueue.QueueId))
					continue;

				var forecastIntervals = forecastProvider.Provide(workloadInfo.WorkloadId);

				if (forecastIntervals.Count == 0)
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
				var queueData = new QueueInterval()
				{
					DateId = interval.DateId,
					IntervalId = interval.IntervalId,
					QueueId = targetQueue.QueueId,
					DatasourceId = targetQueue.DatasourceId,
					OfferedCalls =
						(decimal)Math.Round(
							 interval.Calls + algorithmProperties.CallsConstant * index * (1 - ((double)index / algorithmProperties.IntervalCount)),
							 1),
					HandleTime = (decimal)Math.Round(interval.HandleTime + algorithmProperties.HandleTimeConstant * index)
				};
				queueDataList.Add(queueData);
			}

			return queueDataList;
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
