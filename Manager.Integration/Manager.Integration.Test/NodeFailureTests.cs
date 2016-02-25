using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
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
using NUnit.Framework;


namespace Manager.Integration.Test
{
	[TestFixture, Ignore]
	class NodeFailureTests
	{

		private static readonly ILog Logger =
			LogManager.GetLogger(typeof(NodeFailureTests));

		private bool _clearDatabase = true;
		private string _buildMode = "Debug";


		private string ManagerDbConnectionString { get; set; }

		private Task Task { get; set; }

		private AppDomainTask AppDomainTask { get; set; }

		private CancellationTokenSource CancellationTokenSource { get; set; }

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

		[Test]
		public async void ShouldRemoveNodeWhenDead()
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

			cancellationTokenSource.Cancel();



		}
	}
}
