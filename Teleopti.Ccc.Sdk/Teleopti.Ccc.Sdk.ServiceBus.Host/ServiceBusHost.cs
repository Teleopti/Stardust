using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using Teleopti.Ccc.Domain;
using Teleopti.Wfm.Azure.Common;

namespace Teleopti.Ccc.Sdk.ServiceBus.Host
{
	public partial class ServiceBusHost : ServiceBase
	{
		private ServiceBusRunner serviceBusRunner;

		public ServiceBusHost()
		{
			InitializeComponent();
			serviceBusRunner = new ServiceBusRunner(RequestAdditionalTime, new WfmInstallationEnvironment());
		}

		protected override void OnStart(string[] args)
		{
			try
			{
				serviceBusRunner.Start();
			}
			catch (Exception exception)
			{
				EventLog.WriteEntry(
					string.Format(
						"An exception occurred when starting the Teleopti Service Bus. Exception will be rethrown to bring down the service, but it might still be logged that the service was started successfully. Service recovery will may make more attempts at starting the service. \nThe exception message: {0}. \nThe stack trace: {1}.",
						exception.Message, exception.StackTrace), EventLogEntryType.Error);
				throwExceptionOnAnotherThreadToCrashAndMakeServiceRecoveryWork(exception);
			}
		}

		private void throwExceptionOnAnotherThreadToCrashAndMakeServiceRecoveryWork(Exception exception)
		{
			new Thread(o =>
			{
				Thread.Sleep(1000);
				PreserveStack.For(exception);
				throw exception;
			}).Start();
		}

		protected override void OnStop()
		{
			serviceBusRunner.Stop();
		}
	}
}
