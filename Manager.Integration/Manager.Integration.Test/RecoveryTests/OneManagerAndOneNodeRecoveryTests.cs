using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Tasks;
using NUnit.Framework;

namespace Manager.Integration.Test.RecoveryTests
{
	[TestFixture, Ignore]
	public class OneManagerAndOneNodeRecoveryTests
	{
		private static readonly ILog Logger =
			LogManager.GetLogger(typeof (OneManagerAndOneNodeRecoveryTests));

		private bool _clearDatabase = true;
		private string _buildMode = "Debug";

		private string ManagerDbConnectionString { get; set; }
		private Task Task { get; set; }
		private AppDomainTask AppDomainTask { get; set; }
		private CancellationTokenSource CancellationTokenSource { get; set; }

		private void logMessage(string message)
		{
			LogHelper.LogDebugWithLineNumber(message, Logger);
		}

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			ManagerDbConnectionString =
				ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString;

			var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));
			logMessage("Start TestFixtureSetUp");

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
			                               numberOfNodes: 1,
			                               cancellationTokenSource: CancellationTokenSource);

			Thread.Sleep(TimeSpan.FromSeconds(2));
			logMessage("Finished TestFixtureSetUp");
		}

		private void CurrentDomain_UnhandledException(object sender,
		                                              UnhandledExceptionEventArgs e)
		{
			var exp = e.ExceptionObject as Exception;
			if (exp != null)
			{
				LogHelper.LogFatalWithLineNumber(exp.Message,
				                                 Logger,
				                                 exp);
			}
		}

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
	}
}