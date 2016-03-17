using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using log4net.Repository.Hierarchy;
using Manager.Integration.Test.Constants;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Tasks;
using Manager.Integration.Test.Timers;
using Manager.IntegrationTest.Console.Host.Log4Net.Extensions;
using NUnit.Framework;

namespace Manager.Integration.Test.FunctionalTests
{
	[TestFixture]
	public class OneManagerAndZeroNodesTests
	{
		[TearDown]
		public void TearDown()
		{
		}

		private bool _clearDatabase = true;
		private string _buildMode = "Debug";


		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			ManagerDbConnectionString =
				ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString;

			var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));

#if (DEBUG)
			// Do nothing.
#else
            _clearDatabase = true;
            _buildMode = "Release";
#endif

			if (_clearDatabase)
			{
				DatabaseHelper.TryClearDatabase(ManagerDbConnectionString);
			}

			CancellationTokenSource = new CancellationTokenSource();

			AppDomainTask = new AppDomainTask(_buildMode);

			Task = AppDomainTask.StartTask(numberOfManagers: 1,
			                               numberOfNodes: 0,
			                               cancellationTokenSource: CancellationTokenSource);

			Thread.Sleep(TimeSpan.FromSeconds(2));

			this.Log().DebugWithLineNumber("Finshed TestFixtureSetUp");
		}

		private string ManagerDbConnectionString { get; set; }

		private Task Task { get; set; }

		private AppDomainTask AppDomainTask { get; set; }


		private CancellationTokenSource CancellationTokenSource { get; set; }

		private void CurrentDomain_UnhandledException(object sender,
		                                              UnhandledExceptionEventArgs e)
		{
			var exp = e.ExceptionObject as Exception;

			if (exp != null)
			{
				this.Log().FatalWithLineNumber(exp.Message,
				                                 exp);
			}
		}


		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			this.Log().DebugWithLineNumber("Start TestFixtureTearDown");

			if (AppDomainTask != null)
			{
				AppDomainTask.Dispose();
			}

			this.Log().DebugWithLineNumber("Finished TestFixtureTearDown");
		}

		[Test]
		public void JobsShouldJustBeQueuedIfNoNodesTest()
		{
			this.Log().DebugWithLineNumber("Start.");

			var createNewJobRequests =
				JobHelper.GenerateFastJobParamsRequests(1);

			this.Log().DebugWithLineNumber("( " + createNewJobRequests.Count + " ) jobs will be created.");


			var timeout =
				JobHelper.GenerateTimeoutTimeInSeconds(createNewJobRequests.Count, 10);

			var jobManagerTaskCreators = new List<JobManagerTaskCreator>();

			var checkJobHistoryStatusTimer = new CheckJobHistoryStatusTimer(createNewJobRequests.Count,
			                                                                StatusConstants.SuccessStatus,
			                                                                StatusConstants.DeletedStatus,
			                                                                StatusConstants.FailedStatus,
			                                                                StatusConstants.CanceledStatus);

			foreach (var jobRequestModel in createNewJobRequests)
			{
				var jobManagerTaskCreator = new JobManagerTaskCreator(checkJobHistoryStatusTimer);

				jobManagerTaskCreator.CreateNewJobToManagerTask(jobRequestModel);

				jobManagerTaskCreators.Add(jobManagerTaskCreator);
			}

			var startJobTaskHelper = new StartJobTaskHelper();

			var taskHlp = startJobTaskHelper.ExecuteCreateNewJobTasks(jobManagerTaskCreators,
																	  CancellationTokenSource,
			                                                          timeout);

			checkJobHistoryStatusTimer.ManualResetEventSlim.Wait(timeout);

			Assert.IsTrue(checkJobHistoryStatusTimer.Guids.Count == createNewJobRequests.Count);

			taskHlp.Dispose();

			foreach (var jobManagerTaskCreator in jobManagerTaskCreators)
			{
				jobManagerTaskCreator.Dispose();
			}

			this.Log().DebugWithLineNumber("Finished.");
		}
	}
}