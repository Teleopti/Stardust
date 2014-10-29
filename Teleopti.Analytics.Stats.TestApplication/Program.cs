﻿using System;
using System.Globalization;

namespace Teleopti.Analytics.Stats.TestApplication
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Console.WriteLine("Welcome to queue data generator!");
			Console.WriteLine("\n");
			while (true)
			{
				Console.WriteLine("Nhib data source name:");
				var nhibDataSourceName = Console.ReadLine();
				if (checkExit(nhibDataSourceName)) break;

				Console.WriteLine("Queue data source name:");
				var queueDataSourceName = Console.ReadLine();
				if (checkExit(queueDataSourceName)) break;

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
					Console.WriteLine("You can only generate data for a maximum of " + dateCheck.NumberOfDays + " days. Continue anyway? (Y/N)");
					if (Console.ReadKey().Key != ConsoleKey.Y)
						break;
					amountOfDays = dateCheck.NumberOfDays.ToString(CultureInfo.InvariantCulture);
				}

				var parameters = new QueueDataParameters
				{
					NhibDataSourcename = nhibDataSourceName,
					QueueDataSourceName = queueDataSourceName,
					IntervalLength = int.Parse(intervalLength),
					AmountOfDays = int.Parse(amountOfDays),
					AmountOfQueues = int.Parse(amountOfQueues),
					StartDate = dateCheck.StartDate,
					
				};
				Console.WriteLine("Amount of rows to be posted to mart.fact_queue table is: \n" + rowsToBeGenerated(parameters));
				Console.WriteLine("Start posting queue data by pressing P.");
				if (Console.ReadKey().Key != ConsoleKey.P)
					break;
				var generator = new QueueStatsGenerator();
				generator.CreateAsync(parameters).Wait();
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
			return info != null && info.ToUpper() == "EXIT";
		}
	}
}
