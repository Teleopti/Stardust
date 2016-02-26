using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
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

namespace Manager.Integration.Test
{
	[TestFixture]
	class NodeFailureTests
	{
		private static readonly ILog Logger =
			LogManager.GetLogger(typeof (NodeFailureTests));

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

			if (_clearDatabase)
			{
				DatabaseHelper.TryClearDatabase(ManagerDbConnectionString);
			}

			CancellationTokenSource = new CancellationTokenSource();

			AppDomainTask = new AppDomainTask(_buildMode);

			Task = AppDomainTask.StartTask(numberOfManagers: 1,
			                               numberOfNodes: 1,
			                               cancellationTokenSource: CancellationTokenSource);

			LogHelper.LogDebugWithLineNumber("Finshed TestFixtureSetUp",
			                                 Logger);
		}

		private void WaitForNodeTimeout()
		{
			//MUST BE CHANGED TO FIT CONFIGURATION
			Thread.Sleep(TimeSpan.FromSeconds(30));
		}

		[Test]
		public async void ShouldRemoveNodeWhenDead()
		{
			LogHelper.LogDebugWithLineNumber("Start test.",
			                                 Logger);


			//---------------------------------------------
			// Notify when all 1 nodes are up and running. 
			//---------------------------------------------


			LogHelper.LogDebugWithLineNumber("Waiting for all 1 nodes to start up.",
			                                 Logger);

			var sqlNotiferCancellationTokenSource = new CancellationTokenSource();

			var sqlNotifier = new SqlNotifier(ManagerDbConnectionString);

			var task = sqlNotifier.CreateNotifyWhenNodesAreUpTask(1,
			                                                      sqlNotiferCancellationTokenSource,
			                                                      IntegerValidators.Value1IsLargerThenOrEqualToValue2Validator);
			task.Start();

			sqlNotifier.NotifyWhenAllNodesAreUp.Wait(TimeSpan.FromMinutes(30));

			sqlNotifier.Dispose();

			LogHelper.LogDebugWithLineNumber("All 1 nodes has started.",
			                                 Logger);


			//---------------------------------------------
			// Kill the node.
			//---------------------------------------------
		var cancellationTokenSource = new CancellationTokenSource();

			IHttpSender httpSender = new HttpSender();

				var uriBuilder =
					new UriBuilder(Settings.Default.ManagerIntegrationTestControllerBaseAddress);

				uriBuilder.Path += "appdomain/nodes/" + "Node1.config";

			var uri = uriBuilder.Uri;

				LogHelper.LogDebugWithLineNumber("Start calling Delete Async ( " + uri + " ) ",
				                                 Logger);

				try
				{
				var response = await httpSender.DeleteAsync(uriBuilder.Uri,
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

			cancellationTokenSource.Cancel();

			//---------------------------------------------
			// Wait for timeout, node must be considered dead.
			//---------------------------------------------
			WaitForNodeTimeout();  

			//---------------------------------------------
			// Check if node is dead.
			//---------------------------------------------
			var managerUriBuilder = new ManagerUriBuilder();

			uri = managerUriBuilder.GetNodesUri();
			
			var responseNodes = await httpSender.GetAsync(uri);

				responseNodes.EnsureSuccessStatusCode();
				var ser = responseNodes.Content.ReadAsStringAsync();
				
			var workerNodes = JsonConvert.DeserializeObject<List<WorkerNode>>(ser.Result);

			var node = workerNodes.FirstOrDefault();

			Assert.NotNull(node);
				Assert.IsTrue(node.Alive == "false");  
			}
		}

		private void WaitForNodeTimeout()
		{
			//MUST BE CHANGED TO FIT CONFIGURATION
			Thread.Sleep(TimeSpan.FromSeconds(20));
		}
	}
}
