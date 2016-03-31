using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Intraday.TestApplication
{
	class Program
	{
		static void Main(string[] args)
		{
			IForecastProvider forecastProvider = new ForecastProviderFake();
			IWorkloadQueuesProvider workloadQueuesProvider = new WorkloadQueuesProviderFake();
			IDictionary<int, IList<QueueData>> queueDataDictionary = new Dictionary<int, IList<QueueData>>();
			IQueueDataPersister queueDataPersister = new QueueDataPersisterFake();

			
			var workloads = workloadQueuesProvider.Provide();

			foreach (var workloadInfo in workloads)
			{
				var targetQueue = getTargetQueue(workloadInfo.Queues);

				if (!targetQueue.HasValue)
					continue;

				var forecastIntervals = forecastProvider.Provide(workloadInfo.WorkloadId);
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
						OfferedCalls = Math.Round(interval.Calls + algorithmProperties.CallsConstant * index * (1 - ((double)index / algorithmProperties.IntervalCount)), 1),
						HandleTime = Math.Round(interval.HandleTime + algorithmProperties.HandleTimeConstant * index)
					};
					queueDataList.Add(queueData);
				}
				queueDataDictionary.Add(targetQueue.Value, queueDataList);
			}

			queueDataPersister.Persist(queueDataDictionary);

			Console.ReadKey();
		}

		private static int? getTargetQueue(IEnumerable<QueueInfo> queues)
		{
			foreach (var queue in queues.Where(queue => !queue.HasDataToday))
			{
				return queue.QueueId;
			}
			return null;
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
