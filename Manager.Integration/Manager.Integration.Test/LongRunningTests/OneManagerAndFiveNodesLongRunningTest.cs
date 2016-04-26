using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using log4net.Config;
using Manager.Integration.Test.Data;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Models;
using Manager.Integration.Test.Notifications;
using Manager.Integration.Test.Tasks;
using Manager.Integration.Test.Validators;
using Manager.IntegrationTest.Console.Host.Helpers;
using Manager.IntegrationTest.Console.Host.Interfaces;
using Manager.IntegrationTest.Console.Host.Log4Net.Extensions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Manager.Integration.Test.LongRunningTests
{
	[TestFixture, Ignore]
	public class OneManagerAndFiveNodesLongRunningTest
	{
		private string ManagerDbConnectionString { get; set; }


		private Task Task { get; set; }

		private AppDomainTask AppDomainTask { get; set; }

		private CancellationTokenSource CancellationTokenSource { get; set; }

#if (DEBUG)
		private const bool ClearDatabase = true;
		private const string BuildMode = "Debug";

#else
		private const bool ClearDatabase = true;
		private const string BuildMode = "Release";
#endif

		private void LogMessage(string message)
		{
			this.Log().DebugWithLineNumber(message);
		}

		private const int NumberOfManagers = 6;
		private const int NumberOfNodes = 6;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			ServicePointManager.DefaultConnectionLimit = 200;

			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			ManagerDbConnectionString =
				ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString;

			var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));

			LogMessage("Start TestFixtureSetUp");

			if (ClearDatabase)
			{
				DatabaseHelper.TryClearDatabase(ManagerDbConnectionString);
			}

			CancellationTokenSource = new CancellationTokenSource();

			AppDomainTask = new AppDomainTask(BuildMode);

			Task = AppDomainTask.StartTask(numberOfManagers: NumberOfManagers,
										   numberOfNodes: NumberOfNodes,
										   useLoadBalancerIfJustOneManager: true,
										   cancellationTokenSource: CancellationTokenSource);

			Thread.Sleep(TimeSpan.FromSeconds(2));

			LogMessage("Finished TestFixtureSetUp");
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			LogMessage("Start TestFixtureTearDown");

			if (AppDomainTask != null)
			{
				AppDomainTask.Dispose();
			}

			LogMessage("Finished TestFixtureTearDown");
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var exp = e.ExceptionObject as Exception;

			if (exp != null)
			{
				this.Log().FatalWithLineNumber(exp.Message,
											   exp);
			}
		}

		[Test]
		public void TestConnectionString()
		{
			var connectionString=
				ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString;

			SqlConnection connection;

			using (connection = new SqlConnection(connectionString))
			{
				
			}

			var isOPen = connection.State;

		}

		[Test]
		public void TestNumberOfRequest()
		{
			var sqlNotiferCancellationTokenSource = new CancellationTokenSource();
			var sqlNotifier = new SqlNotifier(ManagerDbConnectionString);

			var task = sqlNotifier.CreateNotifyWhenNodesAreUpTask(NumberOfNodes,
																  sqlNotiferCancellationTokenSource,
																  IntegerValidators.Value1IsLargerThenOrEqualToValue2Validator);
			task.Start();

			sqlNotifier.NotifyWhenAllNodesAreUp.Wait(TimeSpan.FromMinutes(30));
			sqlNotifier.Dispose();

			var mangerUriBuilder = new ManagerUriBuilder();
			var uri = mangerUriBuilder.GetAddToJobQueueUri();

			HttpSender httpSender=new HttpSender();

			//for (int i = 0; i < 100; i++)
			//{
			//	var testJobTimerParams =
			//		new TestJobTimerParams("Loop " + 1,
			//								TimeSpan.FromMilliseconds(200));

			//	var jobParamsToJson = JsonConvert.SerializeObject(testJobTimerParams);

			//	var jobQueueItem = new JobQueueItem
			//	{
			//		Name = "Test" + i,
			//		Serialized = jobParamsToJson,
			//		Type = "NodeTest.JobHandlers.TestJobTimerParams",
			//		CreatedBy = "Anton"
			//	};

			//	var response = httpSender.PostAsync(uri, jobQueueItem).Result;
			//}

			Parallel.For(1, 100, (i) =>
			{
				var testJobTimerParams =
					new TestJobTimerParams("Loop " + 1, TimeSpan.FromMilliseconds(200));

				var jobParamsToJson = JsonConvert.SerializeObject(testJobTimerParams);

				var jobQueueItem = new JobQueueItem
				{
					Name = "Test" + i,
					Serialized = jobParamsToJson,
					Type = "NodeTest.JobHandlers.TestJobTimerParams",
					CreatedBy = "Anton"
				};

				var response = httpSender.PostAsync(uri, jobQueueItem).Result;
			});

			Thread.Sleep(TimeSpan.FromMinutes(60));


		}

		/// <summary>
		///     DO NOT FORGET TO RUN COMMAND BELOW AS ADMINISTRATOR.
		///     netsh http add urlacl url=http://+:9050/ user=everyone listen=yes
		/// </summary>
		[Test]
		public void ShouldBeAbleToCreateManySuccessJobRequestTest()
		{
			this.Log().DebugWithLineNumber("Start.");

			var startedTest = DateTime.UtcNow;

			LogMessage("Waiting for all nodes to start up.");

			var sqlNotiferCancellationTokenSource = new CancellationTokenSource();
			var sqlNotifier = new SqlNotifier(ManagerDbConnectionString);

			var task = sqlNotifier.CreateNotifyWhenNodesAreUpTask(NumberOfNodes,
																  sqlNotiferCancellationTokenSource,
																  IntegerValidators.Value1IsLargerThenOrEqualToValue2Validator);
			task.Start();

			sqlNotifier.NotifyWhenAllNodesAreUp.Wait(TimeSpan.FromMinutes(30));
			sqlNotifier.Dispose();

			LogMessage("All nodes has started.");

			var mangerUriBuilder = new ManagerUriBuilder();
			var uri = mangerUriBuilder.GetAddToJobQueueUri();

			var createdBy = SecurityHelper.GetLoggedInUser();

			IHttpSender httpSender = new HttpSender();

			Task<int> task1 = new Task<int>(() => GenerateJobs("Job From Task1 :",createdBy, uri, httpSender,TimeSpan.FromSeconds(5)));
			Task<int> task2 = new Task<int>(() => GenerateJobs("Job From Task2 :",createdBy, uri, httpSender, TimeSpan.FromSeconds(10)));

			task1.Start();
			task2.Start();

			Task.WaitAll(task1, task2);

			Thread.Sleep(TimeSpan.FromHours(8));

			var endedTest = DateTime.UtcNow;

			var description =
				string.Format("Creates {0} FAST jobs with {1} manager and {2} nodes.",
							  task1.Result,
							  NumberOfManagers,
							  NumberOfNodes);

			DatabaseHelper.AddPerformanceData(ManagerDbConnectionString,
											  description,
											  startedTest,
											  endedTest);
		}

		private int GenerateJobs(string jobName,
								 string createdBy, 
								 Uri uri, 
								 IHttpSender httpSender, 
								 TimeSpan jobDuration)
		{
			var loop = 1;

			while (loop <= 5000)
			{
				loop++;

				var testJobTimerParams = new TestJobTimerParams("Loop " + loop, jobDuration);

				var jobParamsToJson = JsonConvert.SerializeObject(testJobTimerParams);

				var jobQueueItem = new JobQueueItem
				{
					Name = jobName + loop,
					Serialized = jobParamsToJson,
					Type = "NodeTest.JobHandlers.TestJobTimerParams",
					CreatedBy = createdBy
				};

				var createNewJobToManagerSucceeded = false;

				while (!createNewJobToManagerSucceeded)
				{
					this.Log().DebugWithLineNumber(
						"Start calling post async. Uri ( " + uri + " ). Job name : ( " + jobQueueItem.Name + " )");
					try
					{
						var response = httpSender.PostAsync(uri, jobQueueItem).Result;

						createNewJobToManagerSucceeded = response.IsSuccessStatusCode;
					}

					catch
					{
						createNewJobToManagerSucceeded = false;

						this.Log().WarningWithLineNumber(
							"HttpRequestException when calling post async, will soon try again. Uri ( " + uri + " ). Job name : ( " +
							jobQueueItem.Name + " ).");

						Thread.Sleep(TimeSpan.FromSeconds(1));
					}

					Thread.Sleep(TimeSpan.FromSeconds(3));
				}
			}

			return loop;
		}
	}
}