using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Threading;
using Teleopti.Ccc.Intraday.TestCommon;
using Teleopti.Ccc.Intraday.TestCommon.TestApplication;

namespace Teleopti.Ccc.Intraday.TestApplication
{
	class Program
	{
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
			initialize();

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

			var generateIntradayData = new GenerateIntradayData(workloadQueuesProvider, forecastProvider, uniqueQueueProvider,
				queueDataDictionary, queueDataPersister, userTimeZoneProvider, timeZoneprovider);

			var input = goWithFlow ? generateIntradayData.GenerateInput() : collectInput();
			

			generateIntradayData.GenerateData(input);
		}

		private static void initialize()
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
		
		private static IntradayTestInput collectInput()
		{
			var input = new IntradayTestInput
			{
				UserTimeZoneInfo = getUserTimeZone()
			};
			input.TimeZoneInterval = timeZoneprovider.Provide(input.UserTimeZoneInfo.TimeZoneId);
			input.Date = getDateInput();
			var numberOfDaysBack = getNumberOfDaysInput();
			input.TimePeriods = GetTimePeriods(numberOfDaysBack, input.TimeZoneInterval, input.Date);

			return input;
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

				if (DateTime.TryParseExact(input, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
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

		private static UserTimeZoneInfo getUserTimeZone()
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

	}

	
}
