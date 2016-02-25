using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Notifications;
using Manager.Integration.Test.Properties;
using Manager.Integration.Test.Scripts;
using Manager.Integration.Test.Tasks;
using Manager.Integration.Test.Validators;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Manager.Integration.Test
{
	[TestFixture]
	public class IntegrationControllerTests
	{
		private static readonly ILog Logger =
			LogManager.GetLogger(typeof (IntegrationControllerTests));

		private bool _clearDatabase = true;
		private string _buildMode = "Debug";

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			LogHelper.LogDebugWithLineNumber("Start TestFixtureTearDown",
			                                 Logger);

			if (AppDomainTask != null)
			{
				AppDomainTask.Dispose();
			}

			LogHelper.LogDebugWithLineNumber("Finished TestFixtureTearDown",
			                                 Logger);
		}

		private static void TryCreateSqlLoggingTable(string connectionString)
		{
			LogHelper.LogDebugWithLineNumber("Run sql script to create logging file started.",
			                                 Logger);

			var scriptFile =
				new FileInfo(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
				                          Settings.Default.CreateLoggingTableSqlScriptLocationAndFileName));

			ScriptExecuteHelper.ExecuteScriptFile(scriptFile,
			                                      connectionString);

			LogHelper.LogDebugWithLineNumber("Run sql script to create logging file finished.",
			                                 Logger);
		}

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
#if (DEBUG)
			// Do nothing.
#else
            _clearDatabase = true;
            _buildMode = "Release";
#endif
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			ManagerDbConnectionString =
				ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString;

			var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));

			LogHelper.LogDebugWithLineNumber("Start TestFixtureSetUp",
			                                 Logger);

			TryCreateSqlLoggingTable(ManagerDbConnectionString);

			if (_clearDatabase)
			{
				DatabaseHelper.TryClearDatabase(ManagerDbConnectionString);
			}

			CancellationTokenSource = new CancellationTokenSource();

			AppDomainTask = new AppDomainTask(_buildMode);

			Task = AppDomainTask.StartTask(numberOfManagers: 1,
			                               numberOfNodes: 2,
			                               cancellationTokenSource: CancellationTokenSource);

			LogHelper.LogDebugWithLineNumber("Finshed TestFixtureSetUp",
			                                 Logger);
		}

		private static void CurrentDomain_UnhandledException(object sender,
		                                                     UnhandledExceptionEventArgs e)
		{
			var exception = e.ExceptionObject as Exception;

			if (exception != null)
			{
				LogHelper.LogFatalWithLineNumber(exception.Message,
				                                 Logger,
				                                 exception);
			}
		}

		private string ManagerDbConnectionString { get; set; }

		private Task Task { get; set; }

		private AppDomainTask AppDomainTask { get; set; }

		private CancellationTokenSource CancellationTokenSource { get; set; }


		[Test]
		public async void ShouldBeAbleToStartNewManager()
		{
			LogHelper.LogDebugWithLineNumber("Start test.",
			                                 Logger);

			//---------------------------------------------
			// Notify when all 2 nodes are up and running. 
			//---------------------------------------------
			LogHelper.LogDebugWithLineNumber("Waiting for all 2 nodes to start up.",
			                                 Logger);

			var sqlNotiferCancellationTokenSource = new CancellationTokenSource();

			var sqlNotifier = new SqlNotifier(ManagerDbConnectionString);

			var task = sqlNotifier.CreateNotifyWhenNodesAreUpTask(2,
			                                                      sqlNotiferCancellationTokenSource,
			                                                      IntegerValidators.Value1IsLargerThenOrEqualToValue2Validator);
			task.Start();

			sqlNotifier.NotifyWhenAllNodesAreUp.Wait(TimeSpan.FromMinutes(30));

			sqlNotifier.Dispose();

			LogHelper.LogDebugWithLineNumber("All 2 nodes has started.",
			                                 Logger);


			//---------------------------------------------
			// Start actual test.
			//---------------------------------------------
			var cancellationTokenSource = new CancellationTokenSource();

			HttpResponseMessage response = null;

			string managerName = null;

			using (var client = new HttpClient())
			{
				var uriBuilder =
					new UriBuilder(Settings.Default.ManagerIntegrationTestControllerBaseAddress);

				uriBuilder.Path += "appdomain/managers";

				var uri = uriBuilder.Uri;

				LogHelper.LogDebugWithLineNumber("Start calling Post Async ( " + uri + " ) ",
				                                 Logger);

				try
				{
					response = await client.PostAsync(uriBuilder.Uri,
					                                  null,
					                                  cancellationTokenSource.Token);

					if (response.IsSuccessStatusCode)
					{
						managerName = await response.Content.ReadAsStringAsync();

						LogHelper.LogDebugWithLineNumber("Succeeded calling Post Async ( " + uri + " ) ",
						                                 Logger);
					}
				}
				catch (Exception exp)
				{
					LogHelper.LogErrorWithLineNumber(exp.Message,
					                                 Logger,
					                                 exp);
				}
			}

			Assert.IsNotNull(response,
			                 "Response can not be null.");

			Assert.IsTrue(response.IsSuccessStatusCode,
			              "Response code should be success.");

			Assert.IsNotNull(managerName,
			                 "Manager must have a friendly name.");

			cancellationTokenSource.Cancel();

			LogHelper.LogDebugWithLineNumber("Finished test.",
			                                 Logger);
		}

		/// <summary>
		///     DO NOT FORGET TO RUN COMMAND BELOW AS ADMINISTRATOR.
		///     netsh http add urlacl url=http://+:9100/ user=everyone listen=yes
		/// </summary>
		[Test]
		public async void ShouldBeAbleToStartNewNode()
		{
			LogHelper.LogDebugWithLineNumber("Start test.",
			                                 Logger);

			//---------------------------------------------
			// Notify when all 2 nodes are up and running. 
			//---------------------------------------------
			LogHelper.LogDebugWithLineNumber("Waiting for all 2 nodes to start up.",
			                                 Logger);

			var sqlNotiferCancellationTokenSource = new CancellationTokenSource();

			var sqlNotifier = new SqlNotifier(ManagerDbConnectionString);

			var task = sqlNotifier.CreateNotifyWhenNodesAreUpTask(2,
			                                                      sqlNotiferCancellationTokenSource,
			                                                      IntegerValidators.Value1IsLargerThenOrEqualToValue2Validator);
			task.Start();

			sqlNotifier.NotifyWhenAllNodesAreUp.Wait(TimeSpan.FromMinutes(30));

			sqlNotifier.Dispose();

			LogHelper.LogDebugWithLineNumber("All 2 nodes has started.",
			                                 Logger);

			//---------------------------------------------
			// Start actual test.
			//---------------------------------------------
			var cancellationTokenSource = new CancellationTokenSource();

			HttpResponseMessage response = null;

			string nodeName = null;

			using (var client = new HttpClient())
			{
				var uriBuilder =
					new UriBuilder(Settings.Default.ManagerIntegrationTestControllerBaseAddress);

				uriBuilder.Path += "appdomain/nodes";

				var uri = uriBuilder.Uri;

				LogHelper.LogDebugWithLineNumber("Start calling Post Async ( " + uri + " ) ",
				                                 Logger);

				try
				{
					response = await client.PostAsync(uriBuilder.Uri,
					                                  null,
					                                  cancellationTokenSource.Token);

					if (response.IsSuccessStatusCode)
					{
						nodeName = await response.Content.ReadAsStringAsync();

						LogHelper.LogDebugWithLineNumber("Succeeded calling Post Async ( " + uri + " ) ",
						                                 Logger);
					}
				}
				catch (Exception exp)
				{
					LogHelper.LogErrorWithLineNumber(exp.Message,
					                                 Logger,
					                                 exp);
				}
			}

			Assert.IsNotNull(response,
			                 "Response can not be null.");

			Assert.IsTrue(response.IsSuccessStatusCode,
			              "Response code should be success.");

			Assert.IsNotNull(nodeName,
			                 "Node must have a friendly name.");

			cancellationTokenSource.Cancel();

			LogHelper.LogDebugWithLineNumber("Finished test.",
			                                 Logger);
		}


		/// <summary>
		///     DO NOT FORGET TO RUN COMMAND BELOW AS ADMINISTRATOR.
		///     netsh http add urlacl url=http://+:9100/ user=everyone listen=yes
		/// </summary>
		[Test]
		public async void ShouldReturnAllManagers()
		{
			LogHelper.LogDebugWithLineNumber("Start test.",
			                                 Logger);

			//---------------------------------------------
			// Notify when 2 nodes are up. 
			//---------------------------------------------
			LogHelper.LogDebugWithLineNumber("Waiting for 2 nodes to start up.",
			                                 Logger);

			var sqlNotiferCancellationTokenSource = new CancellationTokenSource();

			var sqlNotifier = new SqlNotifier(ManagerDbConnectionString);

			var task = sqlNotifier.CreateNotifyWhenNodesAreUpTask(2,
			                                                      sqlNotiferCancellationTokenSource,
			                                                      IntegerValidators.Value1IsLargerThenOrEqualToValue2Validator);
			task.Start();

			sqlNotifier.NotifyWhenAllNodesAreUp.Wait(TimeSpan.FromMinutes(30));

			sqlNotifier.Dispose();

			LogHelper.LogDebugWithLineNumber("All 2 nodes has started.",
			                                 Logger);

			//---------------------------------------------
			// Start actual test.
			//---------------------------------------------
			HttpResponseMessage response = null;

			var cancellationTokenSource = new CancellationTokenSource();

			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				var uriBuilder =
					new UriBuilder(Settings.Default.ManagerIntegrationTestControllerBaseAddress);

				uriBuilder.Path += "appdomain/managers";

				var uri = uriBuilder.Uri;

				LogHelper.LogDebugWithLineNumber("Start calling Get Async ( " + uri + " ) ",
				                                 Logger);

				try
				{
					response = await client.GetAsync(uriBuilder.Uri,
					                                 cancellationTokenSource.Token);

					if (response.IsSuccessStatusCode)
					{
						LogHelper.LogDebugWithLineNumber("Succeeded calling Get Async ( " + uri + " ) ",
						                                 Logger);

						var content = await response.Content.ReadAsStringAsync();

						var list =
							JsonConvert.DeserializeObject<List<string>>(content);

						if (list.Any())
						{
							foreach (var l in list)
							{
								LogHelper.LogDebugWithLineNumber(l,
								                                 Logger);
							}
						}

						Assert.IsTrue(list.Any(),
						              "Should return a list of managers.");
					}
				}

				catch (Exception exp)
				{
					LogHelper.LogErrorWithLineNumber(exp.Message,
					                                 Logger,
					                                 exp);
				}
			}

			Assert.IsNotNull(response,
			                 "Response can not be null.");

			Assert.IsTrue(response.IsSuccessStatusCode,
			              "Response code should be success.");

			cancellationTokenSource.Cancel();

			task.Dispose();

			LogHelper.LogDebugWithLineNumber("Finished test.",
			                                 Logger);
		}

		/// <summary>
		///     DO NOT FORGET TO RUN COMMAND BELOW AS ADMINISTRATOR.
		///     netsh http add urlacl url=http://+:9100/ user=everyone listen=yes
		/// </summary>
		[Test]
		public async void ShouldReturnAllNodes()
		{
			LogHelper.LogDebugWithLineNumber("Start test.",
			                                 Logger);

			//---------------------------------------------
			// Notify when 2 nodes are up. 
			//---------------------------------------------
			LogHelper.LogDebugWithLineNumber("Waiting for 2 nodes to start up.",
			                                 Logger);

			var sqlNotiferCancellationTokenSource = new CancellationTokenSource();

			var sqlNotifier = new SqlNotifier(ManagerDbConnectionString);

			var task = sqlNotifier.CreateNotifyWhenNodesAreUpTask(2,
			                                                      sqlNotiferCancellationTokenSource,
			                                                      IntegerValidators.Value1IsLargerThenOrEqualToValue2Validator);
			task.Start();

			sqlNotifier.NotifyWhenAllNodesAreUp.Wait(TimeSpan.FromMinutes(30));

			sqlNotifier.Dispose();

			LogHelper.LogDebugWithLineNumber("All 2 nodes has started.",
			                                 Logger);

			//---------------------------------------------
			// Start actual test.
			//---------------------------------------------
			HttpResponseMessage response = null;

			var cancellationTokenSource = new CancellationTokenSource();

			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				var uriBuilder =
					new UriBuilder(Settings.Default.ManagerIntegrationTestControllerBaseAddress);

				uriBuilder.Path += "appdomain/nodes";

				var uri = uriBuilder.Uri;

				LogHelper.LogDebugWithLineNumber("Start calling Get Async ( " + uri + " ) ",
				                                 Logger);

				try
				{
					response = await client.GetAsync(uriBuilder.Uri,
					                                 cancellationTokenSource.Token);

					if (response.IsSuccessStatusCode)
					{
						LogHelper.LogDebugWithLineNumber("Succeeded calling Get Async ( " + uri + " ) ",
						                                 Logger);

						var content = await response.Content.ReadAsStringAsync();

						var list =
							JsonConvert.DeserializeObject<List<string>>(content);

						if (list.Any())
						{
							foreach (var l in list)
							{
								LogHelper.LogDebugWithLineNumber(l,
								                                 Logger);
							}
						}

						Assert.IsTrue(list.Any(),
						              "Should return a list of appdomain keys.");
					}
				}

				catch (Exception exp)
				{
					LogHelper.LogErrorWithLineNumber(exp.Message,
					                                 Logger,
					                                 exp);
				}
			}

			Assert.IsNotNull(response,
			                 "Response can not be null.");

			Assert.IsTrue(response.IsSuccessStatusCode,
			              "Response code should be success.");

			cancellationTokenSource.Cancel();

			task.Dispose();

			LogHelper.LogDebugWithLineNumber("Finished test.",
			                                 Logger);
		}

		[Test]
		public async void ShouldBeAbleToShutDownManager1()
		{
			LogHelper.LogDebugWithLineNumber("Start test.",
											 Logger);

			//---------------------------------------------
			// Notify when all 2 nodes are up and running. 
			//---------------------------------------------
			LogHelper.LogDebugWithLineNumber("Waiting for all 2 nodes to start up.",
											 Logger);

			var sqlNotiferCancellationTokenSource = new CancellationTokenSource();

			var sqlNotifier = new SqlNotifier(ManagerDbConnectionString);

			var task = sqlNotifier.CreateNotifyWhenNodesAreUpTask(2,
																  sqlNotiferCancellationTokenSource,
																  IntegerValidators.Value1IsLargerThenOrEqualToValue2Validator);
			task.Start();

			sqlNotifier.NotifyWhenAllNodesAreUp.Wait(TimeSpan.FromMinutes(30));

			sqlNotifier.Dispose();

			LogHelper.LogDebugWithLineNumber("All 2 nodes has started.",
											 Logger);

			//---------------------------------------------
			// Start actual test.
			//---------------------------------------------
			var cancellationTokenSource = new CancellationTokenSource();

			HttpResponseMessage response = null;

			using (var client = new HttpClient())
			{
				var uriBuilder =
					new UriBuilder(Settings.Default.ManagerIntegrationTestControllerBaseAddress);

				uriBuilder.Path += "appdomain/managers/" + "Manager1.config";

				var uri = uriBuilder.Uri;

				LogHelper.LogDebugWithLineNumber("Start calling Delete Async ( " + uri + " ) ",
												 Logger);

				try
				{
					response = await client.DeleteAsync(uriBuilder.Uri,
														cancellationTokenSource.Token);

					if (response.IsSuccessStatusCode)
					{
						LogHelper.LogDebugWithLineNumber("Succeeded calling Delete Async ( " + uri + " ) ",
														 Logger);
					}
				}
				catch (Exception exp)
				{
					LogHelper.LogErrorWithLineNumber(exp.Message,
													 Logger,
													 exp);
				}
			}

			Assert.IsNotNull(response,
							 "Response can not be null.");

			Assert.IsTrue(response.IsSuccessStatusCode,
						  "Response code should be success.");


			cancellationTokenSource.Cancel();

			LogHelper.LogDebugWithLineNumber("Finished test.",
											 Logger);
		}

		/// <summary>
		///     DO NOT FORGET TO RUN COMMAND BELOW AS ADMINISTRATOR.
		///     netsh http add urlacl url=http://+:9100/ user=everyone listen=yes
		/// </summary>
		[Test]
		public async void ShouldBeAbleToShutDownNode1()
		{
			LogHelper.LogDebugWithLineNumber("Start test.",
			                                 Logger);

			//---------------------------------------------
			// Notify when all 2 nodes are up and running. 
			//---------------------------------------------
			LogHelper.LogDebugWithLineNumber("Waiting for all 2 nodes to start up.",
			                                 Logger);

			var sqlNotiferCancellationTokenSource = new CancellationTokenSource();

			var sqlNotifier = new SqlNotifier(ManagerDbConnectionString);

			var task = sqlNotifier.CreateNotifyWhenNodesAreUpTask(2,
			                                                      sqlNotiferCancellationTokenSource,
			                                                      IntegerValidators.Value1IsLargerThenOrEqualToValue2Validator);
			task.Start();

			sqlNotifier.NotifyWhenAllNodesAreUp.Wait(TimeSpan.FromMinutes(30));

			sqlNotifier.Dispose();

			LogHelper.LogDebugWithLineNumber("All 2 nodes has started.",
			                                 Logger);

			//---------------------------------------------
			// Start actual test.
			//---------------------------------------------
			var cancellationTokenSource = new CancellationTokenSource();

			HttpResponseMessage response = null;

			using (var client = new HttpClient())
			{
				var uriBuilder =
					new UriBuilder(Settings.Default.ManagerIntegrationTestControllerBaseAddress);

				uriBuilder.Path += "appdomain/nodes/" + "Node1.config";

				var uri = uriBuilder.Uri;

				LogHelper.LogDebugWithLineNumber("Start calling Delete Async ( " + uri + " ) ",
				                                 Logger);

				try
				{
					response = await client.DeleteAsync(uriBuilder.Uri,
					                                    cancellationTokenSource.Token);

					if (response.IsSuccessStatusCode)
					{
						LogHelper.LogDebugWithLineNumber("Succeeded calling Delete Async ( " + uri + " ) ",
						                                 Logger);
					}
				}
				catch (Exception exp)
				{
					LogHelper.LogErrorWithLineNumber(exp.Message,
					                                 Logger,
					                                 exp);
				}
			}

			Assert.IsNotNull(response,
			                 "Response can not be null.");

			Assert.IsTrue(response.IsSuccessStatusCode,
			              "Response code should be success.");


			cancellationTokenSource.Cancel();

			LogHelper.LogDebugWithLineNumber("Finished test.",
			                                 Logger);
		}
	}
}