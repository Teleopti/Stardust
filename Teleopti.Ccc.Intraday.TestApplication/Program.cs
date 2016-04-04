using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Intraday.TestApplication
{
	class Program
	{
		static void Main(string[] args)
		{
			const string connectionString = "Data Source=.;Integrated Security=SSPI;Initial Catalog=main_DemoSales_TeleoptiAnalytics;Application Name=Teleopti.Ccc.Intraday.TestApplication";
			IForecastProvider forecastProvider = new ForecastProvider(connectionString);
			IWorkloadQueuesProvider workloadQueuesProvider = new WorkloadQueuesProvider(connectionString);
			IDictionary<int, IList<QueueData>> queueDataDictionary = new Dictionary<int, IList<QueueData>>();
			IQueueDataPersister queueDataPersister = new QueueDataPersisterFake();
			
			var workloads = workloadQueuesProvider.Provide();

			foreach (var workloadInfo in workloads)
			{
				var targetQueue = getTargetQueue(workloadInfo.Queues);
				if (!targetQueue.HasValue)
					continue;

				if(queueDataDictionary.ContainsKey(targetQueue.Value))
					continue;

				var forecastIntervals = forecastProvider.Provide(workloadInfo.WorkloadId);

				queueDataDictionary.Add(targetQueue.Value, generateQueueDataIntervals(forecastIntervals, targetQueue));
			}

			queueDataPersister.Persist(queueDataDictionary);

			Console.ReadKey();
		}

		private static IList<QueueData> generateQueueDataIntervals(IList<ForecastInterval> forecastIntervals, int? targetQueue)
		{
			var queueDataList = new List<QueueData>();
			foreach (var interval in forecastIntervals)
			{
				var algorithmProperties = getAlgorithmProperties(forecastIntervals);

				var index = forecastIntervals.IndexOf(interval);
				var queueData = new QueueData()
				{
					DateId = interval.DateId,
					IntervalId = interval.IntervalId,
					QueueId = targetQueue.Value,
					OfferedCalls =
						Math.Round(
							interval.Calls + algorithmProperties.CallsConstant*index*(1 - ((double) index/algorithmProperties.IntervalCount)),
							1),
					HandleTime = Math.Round(interval.HandleTime + algorithmProperties.HandleTimeConstant*index)
				};
				queueDataList.Add(queueData);
			}

			return queueDataList;
		}

		private static int? getTargetQueue(IEnumerable<QueueInfo> queues)
		{
			var queueInfos = queues as QueueInfo[] ?? queues.ToArray();
			if (queueInfos.Any(x => x.HasDataToday))
				return null;

			return queueInfos.First().QueueId;
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
