using System;
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
		private static readonly object syncLock = new object();
		private static readonly Random random = new Random();
		static void Main(string[] args)
		{
			string analyticsConnectionString = ConfigurationManager.AppSettings["analyticsConnectionString"];
			string appDbConnectonString = ConfigurationManager.AppSettings["appConnectionString"];
			IForecastProvider forecastProvider = new ForecastProvider(analyticsConnectionString);
			IWorkloadQueuesProvider workloadQueuesProvider = new WorkloadQueuesProvider(analyticsConnectionString);
			IDictionary<int, IList<QueueInterval>> queueDataDictionary = new Dictionary<int, IList<QueueInterval>>();
			IQueueDataPersister queueDataPersister = new QueueDataPersister(analyticsConnectionString);
			TimeZoneprovider timeZoneprovider = new TimeZoneprovider(analyticsConnectionString);
			var userTimeZoneProvider = new UserTimeZoneProvider(appDbConnectonString);
			UniqueQueueProvider uniqueQueueProvider = new UniqueQueueProvider(appDbConnectonString, analyticsConnectionString);

			Console.WriteLine("This tool will generate queue statistics for today for forecasted skills.");
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("CHECKLIST:");
			Console.WriteLine("1. Is there forecast data for today for your skills?");
			Console.WriteLine("");
			Console.WriteLine("2. Have you run ETL 'Reload datamart' job for today?");
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("Checklist fulfilled? Press any key to continue.");
			Console.ReadKey();
			Console.WriteLine("");
			Console.WriteLine("");


			var userTimezone = userTimeZoneProvider.GetTimeZoneForCurrentUser();
			var useCurrentUser = false;
			if (userTimezone != null)
			{
				Console.WriteLine($"Use  user '{userTimezone.Username}' with timezone '{userTimezone.TimeZoneId}'. Y/N?");
				useCurrentUser = Console.ReadLine()?.ToUpper() == "Y";
			}
			if (!useCurrentUser)
			{
				userTimezone = getTimeZoneForUser(userTimeZoneProvider);
			}

			var timeZoneIntervalLength = timeZoneprovider.Provide(userTimezone.TimeZoneId);
			var date = getDateInput();
			var numberOfDaysBack = getNumberOfDaysInput();

			Console.WriteLine($"Using timezone '{timeZoneIntervalLength.TimeZoneId}' and date '{date.ToShortDateString()}'");

			var times = new List<DateTime>();
			for (int backDay = 0; backDay >= numberOfDaysBack * -1; backDay--)
			{
				var time = IntervalHelper.GetValidIntervalTime(timeZoneIntervalLength.IntervalLength, date.AddDays(backDay));
				times.Add(time);
			}

			Console.WriteLine("");
			Console.WriteLine("");
			foreach (var t in times)
			{
				var time = t;
				Console.WriteLine("Stats will be generated up until {0}. Enter other time if needed.",
					$"{time.ToShortDateString()} {time.ToShortTimeString()}");
				var timeText = Console.ReadLine();
				if (!string.IsNullOrEmpty(timeText))
				{
					DateTime temp;
					if (!DateTime.TryParse(timeText, Thread.CurrentThread.CurrentCulture, DateTimeStyles.None, out temp))
					{
						Console.WriteLine("{0} is not a valid time. Press any key to exit.", timeText);
						Console.ReadKey();
						return;
					}
					time = time.Date.Add(temp.TimeOfDay);
					time = IntervalHelper.GetValidIntervalTime(timeZoneIntervalLength.IntervalLength, time);
				}

				Console.WriteLine("We're doing stuff. Please hang around...");
				var timeUtc = TimeZoneInfo.Local.SafeConvertTimeToUtc(DateTime.SpecifyKind(time, DateTimeKind.Unspecified));
				var currentIntervalUtc = IntervalHelper.GetIntervalId(timeZoneIntervalLength.IntervalLength, timeUtc);

				var workloads = workloadQueuesProvider.Provide();

				foreach (var workloadInfo in workloads)
				{
					var forecastIntervals = forecastProvider.Provide(workloadInfo.WorkloadId, currentIntervalUtc, timeUtc);

					if (forecastIntervals.Count == 0 || Math.Abs(forecastIntervals.Sum(x => x.Calls)) < 0.001)
						continue;

					var targetQueue = uniqueQueueProvider.Get(workloadInfo);

					queueDataDictionary.Add(targetQueue.QueueId, generateQueueDataIntervals(forecastIntervals, targetQueue));
				}

				queueDataPersister.Persist(queueDataDictionary, timeZoneIntervalLength.TimeZoneId, time);

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

				queueDataDictionary = new Dictionary<int, IList<QueueInterval>>();
			}

			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("We're done! Press any key to exit.");
			Console.ReadKey();
		}

		private static int getNumberOfDaysInput()
		{
			while (true)
			{
				Console.Write("Enter number of days in the past to generate data for (default, selected date only): ");
				var input = Console.ReadLine();
				return int.TryParse(input, out int n) ? n : 0;
			}
		}

		private static DateTime getDateInput()
		{
			while (true)
			{
				Console.Write("Stats will be generated for today. Specify other date if needed (yyyyMMdd): ");
				var input = Console.ReadLine();

				if (string.IsNullOrEmpty(input))
				{
					return DateTime.Now;
				}

				DateTime date;
				if (DateTime.TryParseExact(input, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
				{
					return date.Add(DateTime.Now.TimeOfDay);
				}

				Console.WriteLine("Invalid date format.");
			}
		}

		private static UserTimeZoneInfo getTimeZoneForUser(UserTimeZoneProvider userTimeZoneProvider)
		{
			while (true)
			{
				Console.Write("Enter username: ");
				var username = Console.ReadLine();
				var userTimeZone = userTimeZoneProvider.GetTimeZoneForUser(username);

				if (userTimeZone == null)
				{
					Console.WriteLine("User not found");
					continue;
				}

				Console.WriteLine($"Use user '{userTimeZone.Username}' with timezone '{userTimeZone.TimeZoneId}'. Y/N?");
				if (Console.ReadLine()?.ToUpper() == "Y")
				{
					return userTimeZone;
				}
			}
		}

		private static int RandomNumber(int min, int max)
		{
			lock (syncLock)
			{ // synchronize
				return random.Next(min, max);
			}
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
				var forecastedAverageHandleTime = interval.Calls > 0 ? interval.HandleTime / interval.Calls : 0;
				var offeredCalls = (decimal)Math.Round(
					 (interval.Calls + 1) +
					 algorithmProperties.CallsConstant * index * (1 - ((double)index / algorithmProperties.IntervalCount)),
					 0);
				var answeredCalls = Math.Round((offeredCalls * RandomNumber(80, 100)) / 100, 0);
				var actualHandleTime = forecastedAverageHandleTime * (double)answeredCalls;
				var talkTime = interval.HandleTime > 0 ? actualHandleTime * (interval.TalkTime / interval.HandleTime) : 0;
				var acw = interval.HandleTime > 0 ? actualHandleTime * (interval.AfterTalkTime / interval.HandleTime) : 0;
				var speedOfAnswer = createSpeedOfAnswer(answeredCalls);
				var isASAWithinSL = speedOfAnswer / answeredCalls <= 20;
				var answeredCallsWithinSL = isASAWithinSL
					 ? Math.Round(answeredCalls * RandomNumber(100, 100) / 100, 0)
					 : Math.Round(answeredCalls * RandomNumber(100, 100) / 100, 0);
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

		private static decimal createSpeedOfAnswer(decimal offeredCalls)
		{
			return offeredCalls * RandomNumber(5, 28);
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
