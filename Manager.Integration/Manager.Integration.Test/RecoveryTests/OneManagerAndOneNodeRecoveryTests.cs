using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using log4net.Repository.Hierarchy;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Tasks;
using Manager.IntegrationTest.Console.Host.Log4Net.Extensions;
using NUnit.Framework;

namespace Manager.Integration.Test.RecoveryTests
{
	[TestFixture, Ignore]
	public class OneManagerAndOneNodeRecoveryTests
	{
#if (DEBUG)
		private const bool ClearDatabase = true;
		private const string BuildMode = "Debug";

#else
		private const bool ClearDatabase = true;
		private const string BuildMode = "Release";
#endif

		private string ManagerDbConnectionString { get; set; }
		private Task Task { get; set; }
		private AppDomainTask AppDomainTask { get; set; }
		private CancellationTokenSource CancellationTokenSource { get; set; }

		private void LogMessage(string message)
		{
			this.Log().DebugWithLineNumber(message);
		}

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
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
			Task = AppDomainTask.StartTask(numberOfManagers: 1,
			                               numberOfNodes: 1,
			                               cancellationTokenSource: CancellationTokenSource);

			Thread.Sleep(TimeSpan.FromSeconds(2));
			LogMessage("Finished TestFixtureSetUp");
		}

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
			LogMessage("Start TestFixtureTearDown");

			if (AppDomainTask != null)
			{
				AppDomainTask.Dispose();
			}

			LogMessage("Finished TestFixtureTearDown");
		}
	}
}