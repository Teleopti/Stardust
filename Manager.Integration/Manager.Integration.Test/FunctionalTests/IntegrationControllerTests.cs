//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.IO;
//using System.Linq;
//using System.Net.Http;
//using System.Threading;
//using System.Threading.Tasks;
//using log4net;
//using log4net.Config;
//using Manager.Integration.Test.Helpers;
//using Manager.Integration.Test.Notifications;
//using Manager.Integration.Test.Properties;
//using Manager.Integration.Test.Tasks;
//using Manager.Integration.Test.Validators;
//using Manager.IntegrationTest.Console.Host.Helpers;
//using Manager.IntegrationTest.Console.Host.Interfaces;
//using Newtonsoft.Json;
//using NUnit.Framework;
//using LoggerExtensions = Manager.Integration.Test.Helpers.LoggerExtensions;

//namespace Manager.Integration.Test
//{
//	[TestFixture, Ignore]
//	public class IntegrationControllerTests
//	{
//		private static readonly ILog Logger =
//			LogManager.GetLogger(typeof (IntegrationControllerTests));

//		private bool _clearDatabase = true;
//		private string _buildMode = "Debug";

//		[TearDown]
//		public void TearDown()
//		{
//			LoggerExtensions.DebugWithLineNumber("Start TestFixtureTearDown",
//			                                 Logger);

//			if (AppDomainTask != null)
//			{
//				AppDomainTask.Dispose();
//			}

//			LoggerExtensions.DebugWithLineNumber("Finished TestFixtureTearDown",
//			                                 Logger);
//		}

//		[SetUp]
//		public void SetUp()
//		{
//#if (DEBUG)
//			// Do nothing.
//#else
//            _clearDatabase = true;
//            _buildMode = "Release";
//#endif
//			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

//			ManagerDbConnectionString =
//				ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString;

//			var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
//			XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));

//			LoggerExtensions.DebugWithLineNumber("Start TestFixtureSetUp",
//			                                 Logger);

//			if (_clearDatabase)
//			{
//				DatabaseHelper.TryClearDatabase(ManagerDbConnectionString);
//			}

//			CancellationTokenSource = new CancellationTokenSource();

//			AppDomainTask = new AppDomainTask(_buildMode);

//			Task = AppDomainTask.StartTask(numberOfManagers: 1,
//			                               numberOfNodes: 2,
//			                               cancellationTokenSource: CancellationTokenSource);

//			LoggerExtensions.DebugWithLineNumber("Finshed TestFixtureSetUp",
//			                                 Logger);
//		}

//		private static void CurrentDomain_UnhandledException(object sender,
//		                                                     UnhandledExceptionEventArgs e)
//		{
//			var exception = e.ExceptionObject as Exception;

//			if (exception != null)
//			{
//				LoggerExtensions.FatalWithLineNumber(exception.Message,
//				                                 Logger,
//				                                 exception);
//			}
//		}

//		private string ManagerDbConnectionString { get; set; }

//		private Task Task { get; set; }

//		private AppDomainTask AppDomainTask { get; set; }

//		private CancellationTokenSource CancellationTokenSource { get; set; }

//		[Test, Ignore]
//		public async void ShouldBeAbleToShutDownManager1()
//		{
//			LoggerExtensions.DebugWithLineNumber("Start test.",
//			                                 Logger);

//			//---------------------------------------------
//			// Notify when all 2 nodes are up and running. 
//			//---------------------------------------------
//			LoggerExtensions.DebugWithLineNumber("Waiting for all 2 nodes to start up.",
//			                                 Logger);

//			var sqlNotiferCancellationTokenSource = new CancellationTokenSource();

//			var sqlNotifier = new SqlNotifier(ManagerDbConnectionString);

//			var task = sqlNotifier.CreateNotifyWhenNodesAreUpTask(2,
//			                                                      sqlNotiferCancellationTokenSource,
//			                                                      IntegerValidators.Value1IsLargerThenOrEqualToValue2Validator);
//			task.Start();

//			sqlNotifier.NotifyWhenAllNodesAreUp.Wait(TimeSpan.FromMinutes(30));

//			sqlNotifier.Dispose();

//			LoggerExtensions.InfoWithLineNumber("All 2 nodes has started.",
//			                                 Logger);

//			//---------------------------------------------
//			// Start actual test.
//			//---------------------------------------------
//			var cancellationTokenSource = new CancellationTokenSource();

//			HttpResponseMessage response = null;

//			IHttpSender httpSender = new HttpSender();


//			var uriBuilder =
//				new UriBuilder(Settings.Default.ManagerIntegrationTestControllerBaseAddress);

//			uriBuilder.Path += "appdomain/managers/Manager1.config";

//			var uri = uriBuilder.Uri;

//			LoggerExtensions.DebugWithLineNumber("Start calling Delete Async ( " + uri + " ) ",
//			                                 Logger);

//			try
//			{
//				response = await httpSender.DeleteAsync(uriBuilder.Uri,
//				                                        cancellationTokenSource.Token);

//				if (response.IsSuccessStatusCode)
//				{
//					LoggerExtensions.DebugWithLineNumber("Succeeded calling Delete Async ( " + uri + " ) ",
//					                                 Logger);
//				}
//			}
//			catch (Exception exp)
//			{
//				LoggerExtensions.ErrorWithLineNumber(exp.Message,
//				                                 Logger,
//				                                 exp);
//			}

//			Assert.IsNotNull(response,
//			                 "Response can not be null.");

//			Assert.IsTrue(response.IsSuccessStatusCode,
//			              "Response code should be success.");


//			cancellationTokenSource.Cancel();

//			LoggerExtensions.DebugWithLineNumber("Finished test.",
//			                                 Logger);
//		}

//		/// <summary>
//		///     DO NOT FORGET TO RUN COMMAND BELOW AS ADMINISTRATOR.
//		///     netsh http add urlacl url=http://+:9100/ user=everyone listen=yes
//		/// </summary>
//		[Test]
//		public async void ShouldBeAbleToShutDownNode1()
//		{
//			LoggerExtensions.DebugWithLineNumber("Start test.",
//			                                 Logger);

//			//---------------------------------------------
//			// Notify when all 2 nodes are up and running. 
//			//---------------------------------------------
//			LoggerExtensions.DebugWithLineNumber("Waiting for all 2 nodes to start up.",
//			                                 Logger);

//			var sqlNotiferCancellationTokenSource = new CancellationTokenSource();

//			var sqlNotifier = new SqlNotifier(ManagerDbConnectionString);

//			var task = sqlNotifier.CreateNotifyWhenNodesAreUpTask(2,
//			                                                      sqlNotiferCancellationTokenSource,
//			                                                      IntegerValidators.Value1IsLargerThenOrEqualToValue2Validator);
//			task.Start();

//			sqlNotifier.NotifyWhenAllNodesAreUp.Wait(TimeSpan.FromMinutes(30));

//			sqlNotifier.Dispose();

//			LoggerExtensions.DebugWithLineNumber("All 2 nodes has started.",
//			                                 Logger);

//			//---------------------------------------------
//			// Start actual test.
//			//---------------------------------------------
//			var cancellationTokenSource = new CancellationTokenSource();

//			HttpResponseMessage response = null;

//			IHttpSender httpSender = new HttpSender();

//			var uriBuilder =
//				new UriBuilder(Settings.Default.ManagerIntegrationTestControllerBaseAddress);

//			uriBuilder.Path += "appdomain/nodes/" + "Node1.config";

//			var uri = uriBuilder.Uri;

//			LoggerExtensions.DebugWithLineNumber("Start calling Delete Async ( " + uri + " ) ",
//			                                 Logger);

//			try
//			{
//				response = await httpSender.DeleteAsync(uriBuilder.Uri,
//				                                        cancellationTokenSource.Token);

//				if (response.IsSuccessStatusCode)
//				{
//					LoggerExtensions.DebugWithLineNumber("Succeeded calling Delete Async ( " + uri + " ) ",
//					                                 Logger);
//				}
//			}
//			catch (Exception exp)
//			{
//				LoggerExtensions.ErrorWithLineNumber(exp.Message,
//				                                 Logger,
//				                                 exp);
//			}

//			Assert.IsNotNull(response,
//			                 "Response can not be null.");

//			Assert.IsTrue(response.IsSuccessStatusCode,
//			              "Response code should be success.");


//			cancellationTokenSource.Cancel();

//			LoggerExtensions.DebugWithLineNumber("Finished test.",
//			                                 Logger);
//		}


//		[Test, Ignore]
//		public async void ShouldBeAbleToStartNewManager()
//		{
//			LoggerExtensions.DebugWithLineNumber("Start test.",
//			                                 Logger);

//			//---------------------------------------------
//			// Notify when all 2 nodes are up and running. 
//			//---------------------------------------------
//			LoggerExtensions.DebugWithLineNumber("Waiting for all 2 nodes to start up.",
//			                                 Logger);

//			var sqlNotiferCancellationTokenSource = new CancellationTokenSource();

//			var sqlNotifier = new SqlNotifier(ManagerDbConnectionString);

//			var task = sqlNotifier.CreateNotifyWhenNodesAreUpTask(2,
//			                                                      sqlNotiferCancellationTokenSource,
//			                                                      IntegerValidators.Value1IsLargerThenOrEqualToValue2Validator);
//			task.Start();

//			sqlNotifier.NotifyWhenAllNodesAreUp.Wait(TimeSpan.FromMinutes(30));

//			sqlNotifier.Dispose();

//			LoggerExtensions.InfoWithLineNumber("All 2 nodes has started.",
//			                                 Logger);


//			//---------------------------------------------
//			// Start actual test.
//			//---------------------------------------------
//			HttpResponseMessage response = null;

//			string managerName = null;

//			IHttpSender httpSender = new HttpSender();

//			var uriBuilder =
//				new UriBuilder(Settings.Default.ManagerIntegrationTestControllerBaseAddress);

//			uriBuilder.Path += "appdomain/managers";

//			var uri = uriBuilder.Uri;

//			LoggerExtensions.DebugWithLineNumber("Start calling Post Async ( " + uri + " ) ",
//			                                 Logger);

//			try
//			{
//				response = await httpSender.PostAsync(uriBuilder.Uri,
//				                                      null);

//				if (response.IsSuccessStatusCode)
//				{
//					managerName = await response.Content.ReadAsStringAsync();

//					LoggerExtensions.DebugWithLineNumber("Succeeded calling Post Async ( " + uri + " ) ",
//					                                 Logger);
//				}
//			}

//			catch (Exception exp)
//			{
//				LoggerExtensions.ErrorWithLineNumber(exp.Message,
//				                                 Logger,
//				                                 exp);
//			}

//			Assert.IsNotNull(response,
//			                 "Response can not be null.");

//			Assert.IsTrue(response.IsSuccessStatusCode,
//			              "Response code should be success.");

//			Assert.IsNotNull(managerName,
//			                 "Manager must have a friendly name.");

//			LoggerExtensions.DebugWithLineNumber("Finished test.",
//			                                 Logger);
//		}

//		/// <summary>
//		///     DO NOT FORGET TO RUN COMMAND BELOW AS ADMINISTRATOR.
//		///     netsh http add urlacl url=http://+:9100/ user=everyone listen=yes
//		/// </summary>
//		[Test]
//		public async void ShouldBeAbleToStartNewNode()
//		{
//			LoggerExtensions.DebugWithLineNumber("Start test.",
//			                                 Logger);

//			//---------------------------------------------
//			// Notify when all 2 nodes are up and running. 
//			//---------------------------------------------
//			LoggerExtensions.DebugWithLineNumber("Waiting for all 2 nodes to start up.",
//			                                 Logger);

//			var sqlNotiferCancellationTokenSource = new CancellationTokenSource();

//			var sqlNotifier = new SqlNotifier(ManagerDbConnectionString);

//			var task = sqlNotifier.CreateNotifyWhenNodesAreUpTask(2,
//			                                                      sqlNotiferCancellationTokenSource,
//			                                                      IntegerValidators.Value1IsLargerThenOrEqualToValue2Validator);
//			task.Start();

//			sqlNotifier.NotifyWhenAllNodesAreUp.Wait(TimeSpan.FromMinutes(30));

//			sqlNotifier.Dispose();

//			LoggerExtensions.DebugWithLineNumber("All 2 nodes has started.",
//			                                 Logger);

//			//---------------------------------------------
//			// Start actual test.
//			//---------------------------------------------
//			var cancellationTokenSource = new CancellationTokenSource();

//			HttpResponseMessage response = null;

//			string nodeName = null;

//			IHttpSender httpSender = new HttpSender();

//			var uriBuilder =
//				new UriBuilder(Settings.Default.ManagerIntegrationTestControllerBaseAddress);

//			uriBuilder.Path += "appdomain/nodes";

//			var uri = uriBuilder.Uri;

//			LoggerExtensions.DebugWithLineNumber("Start calling Post Async ( " + uri + " ) ",
//			                                 Logger);

//			try
//			{
//				response = await httpSender.PostAsync(uriBuilder.Uri,
//				                                      null);

//				if (response.IsSuccessStatusCode)
//				{
//					nodeName = await response.Content.ReadAsStringAsync();

//					LoggerExtensions.DebugWithLineNumber("Succeeded calling Post Async ( " + uri + " ) ",
//					                                 Logger);
//				}
//			}
//			catch (Exception exp)
//			{
//				LoggerExtensions.ErrorWithLineNumber(exp.Message,
//				                                 Logger,
//				                                 exp);
//			}

//			Assert.IsNotNull(response,
//			                 "Response can not be null.");

//			Assert.IsTrue(response.IsSuccessStatusCode,
//			              "Response code should be success.");

//			Assert.IsNotNull(nodeName,
//			                 "Node must have a friendly name.");

//			cancellationTokenSource.Cancel();

//			LoggerExtensions.DebugWithLineNumber("Finished test.",
//			                                 Logger);
//		}


//		/// <summary>
//		///     DO NOT FORGET TO RUN COMMAND BELOW AS ADMINISTRATOR.
//		///     netsh http add urlacl url=http://+:9100/ user=everyone listen=yes
//		/// </summary>
//		[Test, Ignore]
//		public async void ShouldReturnAllManagers()
//		{
//			LoggerExtensions.DebugWithLineNumber("Start test.",
//			                                 Logger);

//			//---------------------------------------------
//			// Notify when 2 nodes are up. 
//			//---------------------------------------------
//			LoggerExtensions.DebugWithLineNumber("Waiting for 2 nodes to start up.",
//			                                 Logger);

//			var sqlNotiferCancellationTokenSource = new CancellationTokenSource();

//			var sqlNotifier = new SqlNotifier(ManagerDbConnectionString);

//			var task = sqlNotifier.CreateNotifyWhenNodesAreUpTask(2,
//			                                                      sqlNotiferCancellationTokenSource,
//			                                                      IntegerValidators.Value1IsLargerThenOrEqualToValue2Validator);
//			task.Start();

//			sqlNotifier.NotifyWhenAllNodesAreUp.Wait(TimeSpan.FromMinutes(30));

//			sqlNotifier.Dispose();

//			LoggerExtensions.DebugWithLineNumber("All 2 nodes has started.",
//			                                 Logger);

//			//---------------------------------------------
//			// Start actual test.
//			//---------------------------------------------
//			HttpResponseMessage response = null;

//			IHttpSender httpSender = new HttpSender();

//			var uriBuilder =
//				new UriBuilder(Settings.Default.ManagerIntegrationTestControllerBaseAddress);

//			uriBuilder.Path += "appdomain/managers";

//			var uri = uriBuilder.Uri;

//			LoggerExtensions.DebugWithLineNumber("Start calling Get Async ( " + uri + " ) ",
//			                                 Logger);

//			try
//			{
//				response = await httpSender.GetAsync(uriBuilder.Uri);

//				if (response.IsSuccessStatusCode)
//				{
//					LoggerExtensions.DebugWithLineNumber("Succeeded calling Get Async ( " + uri + " ) ",
//					                                 Logger);

//					var content = await response.Content.ReadAsStringAsync();

//					var list =
//						JsonConvert.DeserializeObject<List<string>>(content);

//					if (list.Any())
//					{
//						foreach (var l in list)
//						{
//							LoggerExtensions.DebugWithLineNumber(l,
//							                                 Logger);
//						}
//					}

//					Assert.IsTrue(list.Any(),
//					              "Should return a list of managers.");
//				}
//			}

//			catch (Exception exp)
//			{
//				LoggerExtensions.ErrorWithLineNumber(exp.Message,
//				                                 Logger,
//				                                 exp);
//			}


//			Assert.IsNotNull(response,
//			                 "Response can not be null.");

//			Assert.IsTrue(response.IsSuccessStatusCode,
//			              "Response code should be success.");

//			task.Dispose();

//			LoggerExtensions.DebugWithLineNumber("Finished test.",
//			                                 Logger);
//		}

//		/// <summary>
//		///     DO NOT FORGET TO RUN COMMAND BELOW AS ADMINISTRATOR.
//		///     netsh http add urlacl url=http://+:9100/ user=everyone listen=yes
//		/// </summary>
//		[Test]
//		public async void ShouldReturnAllNodes()
//		{
//			LoggerExtensions.DebugWithLineNumber("Start test.",
//			                                 Logger);

//			//---------------------------------------------
//			// Notify when 2 nodes are up. 
//			//---------------------------------------------
//			LoggerExtensions.DebugWithLineNumber("Waiting for 2 nodes to start up.",
//			                                 Logger);

//			var sqlNotiferCancellationTokenSource = new CancellationTokenSource();

//			var sqlNotifier = new SqlNotifier(ManagerDbConnectionString);

//			var task = sqlNotifier.CreateNotifyWhenNodesAreUpTask(2,
//			                                                      sqlNotiferCancellationTokenSource,
//			                                                      IntegerValidators.Value1IsLargerThenOrEqualToValue2Validator);
//			task.Start();

//			sqlNotifier.NotifyWhenAllNodesAreUp.Wait(TimeSpan.FromMinutes(30));

//			sqlNotifier.Dispose();

//			LoggerExtensions.DebugWithLineNumber("All 2 nodes has started.",
//			                                 Logger);

//			//---------------------------------------------
//			// Start actual test.
//			//---------------------------------------------
//			HttpResponseMessage response = null;

//			var cancellationTokenSource = new CancellationTokenSource();

//			IHttpSender httpSender = new HttpSender();

//			var uriBuilder =
//				new UriBuilder(Settings.Default.ManagerIntegrationTestControllerBaseAddress);

//			uriBuilder.Path += "appdomain/nodes";

//			var uri = uriBuilder.Uri;

//			LoggerExtensions.DebugWithLineNumber("Start calling Get Async ( " + uri + " ) ",
//			                                 Logger);

//			try
//			{
//				response = await httpSender.GetAsync(uriBuilder.Uri);

//				if (response.IsSuccessStatusCode)
//				{
//					LoggerExtensions.DebugWithLineNumber("Succeeded calling Get Async ( " + uri + " ) ",
//					                                 Logger);

//					var content = await response.Content.ReadAsStringAsync();

//					var list =
//						JsonConvert.DeserializeObject<List<string>>(content);

//					if (list.Any())
//					{
//						foreach (var l in list)
//						{
//							LoggerExtensions.DebugWithLineNumber(l,
//							                                 Logger);
//						}
//					}

//					Assert.IsTrue(list.Any(),
//					              "Should return a list of appdomain keys.");
//				}
//			}

//			catch (Exception exp)
//			{
//				LoggerExtensions.ErrorWithLineNumber(exp.Message,
//				                                 Logger,
//				                                 exp);
//			}

//			Assert.IsNotNull(response,
//			                 "Response can not be null.");

//			Assert.IsTrue(response.IsSuccessStatusCode,
//			              "Response code should be success.");

//			cancellationTokenSource.Cancel();

//			task.Dispose();

//			LoggerExtensions.DebugWithLineNumber("Finished test.",
//			                                 Logger);
//		}
//	}
//}