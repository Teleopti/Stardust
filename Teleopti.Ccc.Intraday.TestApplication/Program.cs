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
		private static string analyticsConnectionString;
		private static string appDbConnectonString;
		private static IForecastProvider forecastProvider;
		private static IWorkloadQueuesProvider workloadQueuesProvider;
		private static IDictionary<int, IList<QueueInterval>> queueDataDictionary;
		private static IQueueDataPersister queueDataPersister;
		private static TimeZoneprovider timeZoneprovider;
		private static UserTimeZoneProvider userTimeZoneProvider;
		private static UniqueQueueProvider uniqueQueueProvider;

		static void Main(string[] args)
		{
			Initialize();

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

			Console.WriteLine("Go with the flow? y/n?");
			var goWithFlow = Console.ReadLine()?.ToUpper() == "Y";

			var input = goWithFlow ? GenerateInput() : CollectInput();
			GenerateIntradayData(input);
		}

		private static void Initialize()
		{
			analyticsConnectionString = ConfigurationManager.AppSettings["analyticsConnectionString"];
			appDbConnectonString = ConfigurationManager.AppSettings["appConnectionString"];
			forecastProvider = new ForecastProvider(analyticsConnectionString);
			workloadQueuesProvider = new WorkloadQueuesProvider(analyticsConnectionString);
			queueDataDictionary = new Dictionary<int, IList<QueueInterval>>();
			queueDataPersister = new QueueDataPersister(analyticsConnectionString);
			timeZoneprovider = new TimeZoneprovider(analyticsConnectionString);
			userTimeZoneProvider = new UserTimeZoneProvider(appDbConnectonString);
			uniqueQueueProvider = new UniqueQueueProvider(appDbConnectonString, analyticsConnectionString);
		}

		private static IntradayTestApplicationInput GenerateInput()
		{
			IntradayTestApplicationInput input;
			input = new IntradayTestApplicationInput();
			input.Date = DateTime.Today;
			input.UserTimeZoneInfo = userTimeZoneProvider.GetTimeZoneForCurrentUser();
			input.TimeZoneInterval = timeZoneprovider.Provide(input.UserTimeZoneInfo.TimeZoneId);
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

		private static IntradayTestApplicationInput CollectInput()
		{
			var input = new IntradayTestApplicationInput();
			input.UserTimeZoneInfo = GetUserTimeZone();
			input.TimeZoneInterval = timeZoneprovider.Provide(input.UserTimeZoneInfo.TimeZoneId);
			input.Date = getDateInput();
			var numberOfDaysBack = getNumberOfDaysInput();
			input.TimePeriods = GetTimePeriods(numberOfDaysBack, input.TimeZoneInterval, input.Date);

			return input;
		}

		private static UserTimeZoneInfo GetUserTimeZone()
		{
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
			return userTimezone;
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
		
		private static int getNumberOfDaysInput()
		{
			while (true)
			{
				Console.Write("Enter number of days in the past to generate data for (default, selected date only): ");
				var input = Console.ReadLine();
				return int.TryParse(input, out int n) ? n : 0;
			}
		}

		private static List<IntradayTestDateTimePeriod> GetTimePeriods(int numberOfDaysBack, TimeZoneInterval timeZoneInterval, DateTime date)
		{
			var times = new List<DateTime>();
			for (int backDay = 0; backDay >= numberOfDaysBack * -1; backDay--)
			{
				var time = IntervalHelper.GetValidIntervalTime(timeZoneInterval.IntervalLength, date.AddDays(backDay));
				times.Add(time);
			}

			var timePeriods = new List<IntradayTestDateTimePeriod>();

			foreach (var t in times)
			{
				DateTime fromTimeLocal = t.Date;
				DateTime toTimeLocal = t;
				if (times.IndexOf(t) == 0)
				{
					Console.WriteLine($"Stats will be generated for {toTimeLocal.ToShortDateString()} up until {toTimeLocal.ToShortTimeString()}. Enter other time if needed.");
				}
				else
				{
					Console.WriteLine($"Stats will be generated for complete {toTimeLocal.ToShortDateString()}. Enter an up until time if needed.");
					toTimeLocal = fromTimeLocal.AddDays(1);
				}
				var timeText = Console.ReadLine();
				if (!string.IsNullOrEmpty(timeText))
				{
					DateTime temp;
					if (!DateTime.TryParse(timeText, Thread.CurrentThread.CurrentCulture, DateTimeStyles.None, out temp))
					{
						Console.WriteLine("{0} is not a valid time. Press any key to exit.", timeText);
						throw new ArgumentException();
					}
					var timeToAdd = temp.TimeOfDay == TimeSpan.Zero
						? temp.TimeOfDay.Add(TimeSpan.FromDays(1))
						: temp.TimeOfDay;
					toTimeLocal = IntervalHelper.GetValidIntervalTime(timeZoneInterval.IntervalLength,
						fromTimeLocal.Add(timeToAdd));
				}

				timePeriods.Add(new IntradayTestDateTimePeriod(fromTimeLocal, toTimeLocal));
			}
			return timePeriods;
		}

		private static void GenerateIntradayData(IntradayTestApplicationInput input)
		{
			Console.WriteLine($"Using timezone '{input.TimeZoneInterval.TimeZoneId}' and date '{input.Date.ToShortDateString()}'");
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("We're doing stuff. Please hang around...");

			foreach (var period in input.TimePeriods)
			{
				var fromDateUtc = TimeZoneInfo.FindSystemTimeZoneById(input.UserTimeZoneInfo.TimeZoneId)
					.SafeConvertTimeToUtc(DateTime.SpecifyKind(period.From, DateTimeKind.Unspecified));
				var fromIntervalIdUtc = IntervalHelper.GetIntervalId(input.TimeZoneInterval.IntervalLength, fromDateUtc);
				var toDateUtc = TimeZoneInfo.FindSystemTimeZoneById(input.UserTimeZoneInfo.TimeZoneId)
					.SafeConvertTimeToUtc(DateTime.SpecifyKind(period.To, DateTimeKind.Unspecified));
				var toIntervalIdUtc = IntervalHelper.GetIntervalId(input.TimeZoneInterval.IntervalLength, toDateUtc);
				var deleteToDateUtc = TimeZoneInfo.FindSystemTimeZoneById(input.UserTimeZoneInfo.TimeZoneId)
					.SafeConvertTimeToUtc(DateTime.SpecifyKind(period.From.AddDays(1), DateTimeKind.Unspecified));
				var deleteToIntervalIdUtc = IntervalHelper.GetIntervalId(input.TimeZoneInterval.IntervalLength, deleteToDateUtc);

				var workloads = workloadQueuesProvider.Provide();

				foreach (var workloadInfo in workloads)
				{
					var forecastIntervals = forecastProvider.Provide(workloadInfo.WorkloadId, fromDateUtc.Date, fromIntervalIdUtc, toDateUtc.Date, toIntervalIdUtc);

					if (forecastIntervals.Count == 0 || Math.Abs(forecastIntervals.Sum(x => x.Calls)) < 0.001)
						continue;

					var targetQueue = uniqueQueueProvider.Get(workloadInfo);
					queueDataDictionary.Add(targetQueue.QueueId, generateQueueDataIntervals(forecastIntervals, targetQueue));
				}

				queueDataPersister.Persist(queueDataDictionary, fromDateUtc.Date, fromIntervalIdUtc, deleteToDateUtc.Date, deleteToIntervalIdUtc);

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
					Console.WriteLine($"No queue stats generated {period.From:yyyy-MM-dd}. Probably you did not fulfill the checklist.");
				}

				queueDataDictionary = new Dictionary<int, IList<QueueInterval>>();
				Console.WriteLine("");
			}

			Console.WriteLine("We're done! Press any key to exit.");
			Console.ReadKey();
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
