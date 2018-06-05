using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.Intraday.TestCommon
{
	public class GenerateIntradayData
	{
		private readonly IWorkloadQueuesProvider _workloadQueuesProvider;
		private readonly IForecastProvider _forecastProvider;
		private readonly UniqueQueueProvider _uniqueQueueProvider;
		private IDictionary<int, IList<QueueInterval>> _queueDataDictionary;
		private readonly IQueueDataPersister _queueDataPersister;

		private readonly object syncLock = new object();
		private readonly Random random = new Random();
		private readonly UserTimeZoneProvider _userTimeZoneProvider;
		private readonly TimeZoneprovider _timeZoneprovider;

		public GenerateIntradayData(IWorkloadQueuesProvider workloadQueuesProvider,
			IForecastProvider forecastProvider, UniqueQueueProvider uniqueQueueProvider,
			IDictionary<int, IList<QueueInterval>> queueDataDictionary, IQueueDataPersister queueDataPersister,
			UserTimeZoneProvider userTimeZoneProvider, TimeZoneprovider timeZoneprovider)
		{
			_workloadQueuesProvider = workloadQueuesProvider;
			_forecastProvider = forecastProvider;
			_uniqueQueueProvider = uniqueQueueProvider;
			_queueDataDictionary = queueDataDictionary;
			_queueDataPersister = queueDataPersister;
			_userTimeZoneProvider = userTimeZoneProvider;
			_timeZoneprovider = timeZoneprovider;
		}

		
		public void GenerateData(IntradayTestInput input, bool consoleApplication = true)
		{
			if (consoleApplication)
			{
				Console.WriteLine(
					$"Using timezone '{input.TimeZoneInterval.TimeZoneId}' and date '{input.Date.ToShortDateString()}'");
				Console.WriteLine("");
				Console.WriteLine("");
				Console.WriteLine("We're doing stuff. Please hang around...");
			}

			foreach (var period in input.TimePeriods)
			{
				var fromDateUtc = period.From;
				var toDateUtc = period.To;
				var deleteToDateUtc = period.From.AddDays(1);

				if (input.UserTimeZoneInfo.TimeZoneId != "0")
				{
					fromDateUtc = TimeZoneInfo.FindSystemTimeZoneById(input.UserTimeZoneInfo.TimeZoneId)
						.SafeConvertTimeToUtc(DateTime.SpecifyKind(period.From, DateTimeKind.Unspecified));
					toDateUtc = TimeZoneInfo.FindSystemTimeZoneById(input.UserTimeZoneInfo.TimeZoneId)
						.SafeConvertTimeToUtc(DateTime.SpecifyKind(period.To, DateTimeKind.Unspecified));
					deleteToDateUtc = TimeZoneInfo.FindSystemTimeZoneById(input.UserTimeZoneInfo.TimeZoneId)
						.SafeConvertTimeToUtc(DateTime.SpecifyKind(period.From.AddDays(1), DateTimeKind.Unspecified));
				}

				var fromIntervalIdUtc = IntervalHelper.GetIntervalId(input.TimeZoneInterval.IntervalLength, fromDateUtc);
				var toIntervalIdUtc = IntervalHelper.GetIntervalId(input.TimeZoneInterval.IntervalLength, toDateUtc);
				var deleteToIntervalIdUtc = IntervalHelper.GetIntervalId(input.TimeZoneInterval.IntervalLength, deleteToDateUtc);

				var workloads = _workloadQueuesProvider.Provide();

				foreach (var workloadInfo in workloads)
				{
					var forecastIntervals = _forecastProvider.Provide(workloadInfo.WorkloadId, fromDateUtc.Date, fromIntervalIdUtc, toDateUtc.Date, toIntervalIdUtc);

					if (forecastIntervals.Count == 0 || Math.Abs(forecastIntervals.Sum(x => x.Calls)) < 0.001)
						continue;

					var targetQueue = _uniqueQueueProvider.Get(workloadInfo);
					_queueDataDictionary.Add(targetQueue.QueueId, generateQueueDataIntervals(forecastIntervals, targetQueue));
				}

				_queueDataPersister.Persist(_queueDataDictionary, fromDateUtc.Date, fromIntervalIdUtc, deleteToDateUtc.Date, deleteToIntervalIdUtc);

				var skillsContainingQueue = new List<string>();

				foreach (var queueId in _queueDataDictionary.Keys)
				{
					foreach (var workload in workloads)
					{
						foreach (var queue in workload.Queues)
						{
							if (queueId != queue.QueueId) continue;
							if (!skillsContainingQueue.Contains(workload.SkillName))
							{
								skillsContainingQueue.Add(workload.SkillName);
							}
						}
					}
				}

				skillsContainingQueue.Sort();

				if (consoleApplication)
				{
					var skillsAffectedText = new StringBuilder();
					foreach (var skillName in skillsContainingQueue)
					{
						skillsAffectedText.Append(skillName + " | ");
					}

					if (skillsAffectedText.Length > 0)
					{
						Console.WriteLine($"Queue stats were generated {period.From:yyyy-MM-dd} for the following skills:");
						Console.WriteLine(skillsAffectedText);
					}
					else
					{
						Console.WriteLine(
							$"No queue stats generated {period.From:yyyy-MM-dd}. Probably you did not fulfill the checklist.");
					}

					Console.WriteLine("");
				}

				_queueDataDictionary = new Dictionary<int, IList<QueueInterval>>();
			}

			if (!consoleApplication) return;

			Console.WriteLine("We're done! Press any key to exit.");
			Console.ReadKey();
		}

		public IntradayTestInput GenerateInput(UserTimeZoneInfo userTimeZoneInfo = null)
		{
			var input = new IntradayTestInput();
			try
			{
				input.Date = DateTime.Today;
				input.UserTimeZoneInfo = userTimeZoneInfo ?? _userTimeZoneProvider.GetTimeZoneForCurrentUser();
				input.TimeZoneInterval = _timeZoneprovider.Provide(input.UserTimeZoneInfo.TimeZoneId);
				input.TimePeriods = new List<IntradayTestDateTimePeriod>()
				{
					new IntradayTestDateTimePeriod(DateTime.Today, DateTime.Now),
					new IntradayTestDateTimePeriod(DateTime.Today.AddDays(-1), DateTime.Today),
					new IntradayTestDateTimePeriod(DateTime.Today.AddDays(-2), DateTime.Today.AddDays(-1)),
					new IntradayTestDateTimePeriod(DateTime.Today.AddDays(-3), DateTime.Today.AddDays(-2)),
					new IntradayTestDateTimePeriod(DateTime.Today.AddDays(-4), DateTime.Today.AddDays(-3)),
					new IntradayTestDateTimePeriod(DateTime.Today.AddDays(-5), DateTime.Today.AddDays(-4)),
					new IntradayTestDateTimePeriod(DateTime.Today.AddDays(-6), DateTime.Today.AddDays(-5)),
					new IntradayTestDateTimePeriod(DateTime.Today.AddDays(-7), DateTime.Today.AddDays(-6)),
				};
				return input;
			}
			catch (Exception)
			{
				Console.WriteLine("Cant autogenerate input.");
				Console.WriteLine("");
				return null;
			}
		}

		private IList<QueueInterval> generateQueueDataIntervals(IList<ForecastInterval> forecastIntervals, QueueInfo targetQueue)
		{
			var queueDataList = new List<QueueInterval>();

			foreach (var interval in forecastIntervals)
			{
				if (Math.Abs(interval.Calls) < 0.0001)
					continue;

				var algorithmProperties = getAlgorithmProperties(forecastIntervals);

				var index = forecastIntervals.IndexOf(interval);
				var forecastedAverageHandleTime = interval.Calls > 0 ? interval.HandleTime / interval.Calls : 0;
				var offeredCalls = (decimal)Math.Round(
					 (interval.Calls + 1) +
					 algorithmProperties.CallsConstant * index * (1 - ((double)index / algorithmProperties.IntervalCount)),
					 0);
				var answeredCalls = Math.Round((offeredCalls * randomNumber(80, 100)) / 100, 0);
				var actualHandleTime = forecastedAverageHandleTime * (double)answeredCalls;
				var talkTime = interval.HandleTime > 0 ? actualHandleTime * (interval.TalkTime / interval.HandleTime) : 0;
				var acw = interval.HandleTime > 0 ? actualHandleTime * (interval.AfterTalkTime / interval.HandleTime) : 0;
				var speedOfAnswer = createSpeedOfAnswer(answeredCalls);
				var isASAWithinSL = speedOfAnswer / answeredCalls <= 20;
				var answeredCallsWithinSL = isASAWithinSL
					 ? Math.Round(answeredCalls * randomNumber(100, 100) / 100, 0)
					 : Math.Round(answeredCalls * randomNumber(100, 100) / 100, 0);
				var queueData = new QueueInterval()
				{
					DateId = interval.DateId,
					IntervalId = interval.IntervalId,
					QueueId = targetQueue.QueueId,
					DatasourceId = targetQueue.DatasourceId,
					OfferedCalls = offeredCalls,
					AnsweredCalls = answeredCalls,
					AbandonedCalls = offeredCalls - answeredCalls,
					TalkTime = (decimal)talkTime,
					Acw = (decimal)acw,
					HandleTime = new decimal(talkTime + acw),
					SpeedOfAnswer = speedOfAnswer,
					AnsweredCallsWithinSL = answeredCallsWithinSL
				};
				queueDataList.Add(queueData);
			}

			return queueDataList;
		}

		private AlgorithmProperties getAlgorithmProperties(IList<ForecastInterval> forecastIntervals)
		{
			var algorithmProperties = new AlgorithmProperties();
			algorithmProperties.IntervalCount = forecastIntervals.Count;

			var midIntervalCalls = forecastIntervals[algorithmProperties.IntervalCount / 2].Calls;
			algorithmProperties.CallsConstant = midIntervalCalls / algorithmProperties.IntervalCount;

			var handleTimesAverage = forecastIntervals.Select(intervalData => intervalData.HandleTime).ToList().Average();
			algorithmProperties.HandleTimeConstant = handleTimesAverage * 0.03;

			return algorithmProperties;
		}

		private int randomNumber(int min, int max)
		{
			lock (syncLock)
			{ // synchronize
				return random.Next(min, max);
			}
		}

		private decimal createSpeedOfAnswer(decimal offeredCalls)
		{
			return offeredCalls * randomNumber(5, 28);
		}

		internal class AlgorithmProperties
		{
			public int IntervalCount { get; set; }
			public double CallsConstant { get; set; }
			public double HandleTimeConstant { get; set; }
		}

	}
}
