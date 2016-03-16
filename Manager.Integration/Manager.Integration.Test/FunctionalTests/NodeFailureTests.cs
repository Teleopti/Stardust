using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Models;
using Manager.Integration.Test.Notifications;
using Manager.Integration.Test.Properties;
using Manager.Integration.Test.Tasks;
using Manager.Integration.Test.Validators;
using Manager.IntegrationTest.Console.Host.Helpers;
using Manager.IntegrationTest.Console.Host.Interfaces;
using Newtonsoft.Json;
using NUnit.Framework;
using LogHelper = Manager.Integration.Test.Helpers.LogHelper;

namespace Manager.Integration.Test.FunctionalTests
{
	[TestFixture]
	class NodeFailureTests
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (NodeFailureTests));

		private bool _clearDatabase = true;
		private string _buildMode = "Debug";
		private string ManagerDbConnectionString { get; set; }
		private Task Task { get; set; }
		private AppDomainTask AppDomainTask { get; set; }
		private CancellationTokenSource CancellationTokenSource { get; set; }

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			
			logMessage("Start TestFixtureTearDown");
			if (AppDomainTask != null)
			{
				AppDomainTask.Dispose();
			}
			logMessage("Finished TestFixtureTearDown");
		}

		private void logMessage(string message)
		{
			LogHelper.LogDebugWithLineNumber(message, Logger);
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
			ManagerDbConnectionString = ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString;
			var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));
			logMessage("Start TestFixtureSetUp");

			if (_clearDatabase)
			{
				DatabaseHelper.TryClearDatabase(ManagerDbConnectionString);
			}

			CancellationTokenSource = new CancellationTokenSource();
			AppDomainTask = new AppDomainTask(_buildMode);
			Task = AppDomainTask.StartTask(numberOfManagers: 1,
			                               numberOfNodes: 1,
			                               cancellationTokenSource: CancellationTokenSource);

			logMessage("Finshed TestFixtureSetUp");
		}

		private void WaitForNodeTimeout()
		{
			//MUST BE CHANGED TO FIT CONFIGURATION
			Thread.Sleep(TimeSpan.FromSeconds(120));
			// = (alloweddowntime * 2)
		}

		[Test]
		public async void ShouldConsiderNodeAsDeadWhenInactiveAndSetJobResulToFatal()
		{
			logMessage("Start test.");
			//---------------------------------------------
			// Notify when all 1 nodes are up and running. 
			//---------------------------------------------
			logMessage("Waiting for all 1 nodes to start up.");

			var sqlNotiferCancellationTokenSource = new CancellationTokenSource();
			var sqlNotifier = new SqlNotifier(ManagerDbConnectionString);

			var task = sqlNotifier.CreateNotifyWhenNodesAreUpTask(1,
																  sqlNotiferCancellationTokenSource,
																  IntegerValidators.Value1IsLargerThenOrEqualToValue2Validator);
			task.Start();

			sqlNotifier.NotifyWhenAllNodesAreUp.Wait(TimeSpan.FromMinutes(2));
			sqlNotifier.Dispose();

			logMessage("All 1 nodes has started.");

			//---------------------------------------------
			// Send a Job.
			//---------------------------------------------
			IHttpSender httpSender = new HttpSender();

			var managerUriBuilder = new ManagerUriBuilder();
			var uri = managerUriBuilder.GetStartJobUri();

			var createNewJobRequests =
				JobHelper.GenerateTestJobParamsRequests(1);
			HttpResponseMessage response = await httpSender.PostAsync(uri, createNewJobRequests.FirstOrDefault());

			response.EnsureSuccessStatusCode();
			var ser = await response.Content.ReadAsStringAsync();

			var jobId = JsonConvert.DeserializeObject<Guid>(ser);

			//Quick Fix. Should wait for job to be started instead (check DB)
			Thread.Sleep(TimeSpan.FromSeconds(5));
			//---------------------------------------------
			// Kill the node.
			//---------------------------------------------
			var cancellationTokenSource = new CancellationTokenSource();

			var uriBuilder =
				new UriBuilder(Settings.Default.ManagerIntegrationTestControllerBaseAddress);
			uriBuilder.Path += "appdomain/nodes/" + "Node1.config";
			uri = uriBuilder.Uri;

			logMessage("Start calling Delete Async ( " + uri + " ) ");

			try
			{
				response = await httpSender.DeleteAsync(uriBuilder.Uri,
				                                            cancellationTokenSource.Token);
				if (response.IsSuccessStatusCode)
				{
					logMessage("Succeeded calling Delete Async ( " + uri + " ) ");
				}
			}
			catch (Exception exp)
			{
				LogHelper.LogErrorWithLineNumber(exp.Message,
				                                 Logger,
				                                 exp);
			}

			cancellationTokenSource.Cancel();

			//---------------------------------------------
			// Wait for timeout, node must be considered dead.
			//---------------------------------------------
			WaitForNodeTimeout();
			//---------------------------------------------
			// Check if node is dead.
			//---------------------------------------------

			uri = managerUriBuilder.GetNodesUri();

			response = await httpSender.GetAsync(uri);
			response.EnsureSuccessStatusCode();

			ser = await response.Content.ReadAsStringAsync();

			var workerNodes = JsonConvert.DeserializeObject<IList<WorkerNode>>(ser);
			var node = workerNodes.FirstOrDefault();

			Assert.NotNull(node);
			Assert.IsTrue(node.Alive == "false");

			uri = managerUriBuilder.GetJobHistoryUri(jobId);
			response = await httpSender.GetAsync(uri);
			response.EnsureSuccessStatusCode();

			ser = await response.Content.ReadAsStringAsync();
			var jobHistory = JsonConvert.DeserializeObject<JobHistory>(ser);

			Assert.NotNull(jobHistory);
			Assert.IsTrue(jobHistory.Result == "Fatal Node Failure");
		}
	}
}
