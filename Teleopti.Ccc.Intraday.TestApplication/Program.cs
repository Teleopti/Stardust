using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.Intraday.TestApplication
{
	class Program
	{
		static void Main(string[] args)
		{
			IForecastProvider forecastProvider = new ForecastProviderFake();
			IQueueInfoProvider queueInfoProvider = new QueueInfoProviderFake();
			IDictionary<int, IList<QueueData>> queueDataDictionary = new Dictionary<int, IList<QueueData>>();
			IQueueDataPersister queueDataPersister = new QueueDataPersisterFake();

			var forecastData = forecastProvider.Provide();
			
			var queues = queueInfoProvider.Provide();

			foreach (var dataDictionary in forecastData)
			{
				var workloadData = dataDictionary.Value;
				foreach (int queue in workloadData.Queues)
				{
					if (queues.Any(x => x.QueueId == queue && x.HasDataToday)) continue;
					var queueDataList = new List<QueueData>();

					var algorithmProperties = getAlgorithmProperties(workloadData.ForecastIntervals);

					foreach (var intervalData in workloadData.ForecastIntervals)
					{
						var index = workloadData.ForecastIntervals.IndexOf(intervalData);
						var queueData = new QueueData()
						{
							DateId = intervalData.DateId,
							IntervalId = intervalData.IntervalId,
							QueueId = queue,
							OfferedCalls = Math.Round(intervalData.Calls + algorithmProperties.CallsConstant * index * (1 - ((double)index / algorithmProperties.IntervalCount)),1),
							HandleTime = Math.Round(intervalData.HandleTime + algorithmProperties.HandleTimeConstant * index)
						};
						queueDataList.Add(queueData);

					}
					queueDataDictionary.Add(queue, queueDataList);
					break;
				}
			}

			queueDataPersister.Persist(queueDataDictionary);

			Console.ReadKey();
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

	internal interface IQueueDataPersister
	{
		void Persist(IDictionary<int, IList<QueueData>> queueData);
	}

	public class QueueDataPersisterFake : IQueueDataPersister
	{
		public void Persist(IDictionary<int, IList<QueueData>> queueData)
		{
			foreach (var data in queueData)
			{
				var queueTextCalls = new StringBuilder();
				var queueTextHandleTime = new StringBuilder();
				foreach (var queueStats in data.Value)
				{
					queueTextCalls.AppendLine(queueStats.OfferedCalls + ", ");
					queueTextHandleTime.AppendLine(queueStats.HandleTime + ", ");
				}
				Console.WriteLine("Queue: " + data.Key);
				Console.WriteLine(queueTextCalls);
				Console.WriteLine(queueTextHandleTime);
			}
		}
	}

	public class QueueData
	{
		public int QueueId { get; set; }
		public int DateId { get; set; }
		public int IntervalId { get; set; }
		public double OfferedCalls { get; set; }
		public double HandleTime { get; set; }
	}

	public class ForecastProviderFake : IForecastProvider
	{
		public IDictionary<int, ForecastData> Provide()
		{
			var dic = new Dictionary<int, ForecastData>();
			var forecastData = new ForecastData
			{
				ForecastIntervals = getForecastIntervals(),
				Queues = new[] { 1, 2, 3 }
			};
			dic.Add(1, forecastData);
			return dic;
		}

		private IList<ForecastInterval> getForecastIntervals()
		{
			return new ForecastInterval[]
			{
				new ForecastInterval
				{
					DateId = 1,
					IntervalId = 32,
					Calls = 8.2,
					HandleTime = 1200
				},
				new ForecastInterval
				{
					DateId = 1,
					IntervalId = 33,
					Calls = 10,
					HandleTime = 900
				},
				new ForecastInterval
				{
					DateId = 1,
					IntervalId = 34,
					Calls = 11.8,
					HandleTime = 1100
				},
				new ForecastInterval
				{
					DateId = 1,
					IntervalId = 35,
					Calls = 13,
					HandleTime = 1000
				},
				new ForecastInterval
				{
					DateId = 1,
					IntervalId = 36,
					Calls = 15.1,
					HandleTime = 1100
				},
				new ForecastInterval
				{
					DateId = 1,
					IntervalId = 37,
					Calls = 17.2,
					HandleTime = 1200
					},
				new ForecastInterval
				{
					DateId = 1,
					IntervalId = 38,
					Calls = 19.3,
					HandleTime = 1200
					},
				new ForecastInterval
				{
					DateId = 1,
					IntervalId = 39,
					Calls = 20.6,
					HandleTime = 1100
				},

			};
		}
	}
}
