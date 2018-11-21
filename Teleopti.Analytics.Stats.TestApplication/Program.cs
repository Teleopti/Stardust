using System;
using System.Configuration;
using System.Globalization;

namespace Teleopti.Analytics.Stats.TestApplication
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			bool useLatency = false;

			Console.WriteLine("Welcome to queue data generator!");
			Console.WriteLine("\n");

			while (true)
			{
				Console.WriteLine("Simulate latency (web api request {0} ms + db {1} ms)? [Y/N])",
					ConfigurationManager.AppSettings["Latency"],
					ConfigurationManager.AppSettings["DbLatency"]);
				if (Console.ReadKey().Key == ConsoleKey.Y)
					useLatency = true;

				Console.WriteLine("\nNhib data source name:");
				var nhibDataSourceName = Console.ReadLine();
				if (checkExit(nhibDataSourceName)) break;

				Console.WriteLine("Queue data source id:");
				var queueDataSourceId = Console.ReadLine();
				if (checkExit(queueDataSourceId)) break;

				Console.WriteLine("Interval length:");
				var intervalLength = Console.ReadLine();
				if (checkExit(intervalLength)) break;

				Console.WriteLine("Amount of queues:");
				var amountOfQueues = Console.ReadLine();
				if (checkExit(amountOfQueues)) break;

				Console.WriteLine("Amount of days:");
				var amountOfDays = Console.ReadLine();
				if (checkExit(amountOfDays)) break;

				Console.WriteLine("Checking data in mart.dim_date table...");
				var db = new DatabaseConnectionHandler();
				var dateCheck = db.GetNumberOfDaysInDimDateTable();
				if (dateCheck.NumberOfDays == -1)
				{
					Console.WriteLine("No dates found in mart.dim_date. The Initial job in ETL must first be executed. Press any key to exit.");
					Console.ReadKey();
					break;
				}
				if (dateCheck.NumberOfDays == -99)
				{
					Console.WriteLine("Call to Analytics database failed. Check configuration file. Press any key to exit.");
					Console.ReadKey();
					break;
				}
				if (dateCheck.NumberOfDays < int.Parse(amountOfDays))
				{
					Console.WriteLine("You can only generate data for a maximum of " + dateCheck.NumberOfDays + " days. Continue anyway? [Y/N]");
					if (Console.ReadKey().Key != ConsoleKey.Y)
						break;
					amountOfDays = dateCheck.NumberOfDays.ToString(CultureInfo.InvariantCulture);
				}

				var parameters = new QueueDataParameters
				{
					NhibDataSourcename = nhibDataSourceName,
					QueueDataSourceId = int.Parse(queueDataSourceId),
					IntervalLength = int.Parse(intervalLength),
					AmountOfDays = int.Parse(amountOfDays),
					AmountOfQueues = int.Parse(amountOfQueues),
					StartDate = dateCheck.StartDate,
					UseLatency = useLatency
				};
				Console.WriteLine("\nAmount of rows to be posted to mart.fact_queue table is: \n" + rowsToBeGenerated(parameters));
				Console.WriteLine("Start posting queue data by pressing P.");
				if (Console.ReadKey().Key != ConsoleKey.P)
					break;
				var generator = new QueueStatsGenerator();
				generator.CreateAsync(parameters).GetAwaiter().GetResult();
				Console.ReadLine();
				break;
			}
		}

		private static int rowsToBeGenerated(QueueDataParameters queueDataParameters)
		{
			var intervalPerDay = 1440/queueDataParameters.IntervalLength;
			return intervalPerDay * queueDataParameters.AmountOfDays * queueDataParameters.AmountOfQueues;
		}

		private static bool checkExit(string info)
		{
			return info != null && info.Equals("EXIT",StringComparison.CurrentCultureIgnoreCase);
		}
	}
}
