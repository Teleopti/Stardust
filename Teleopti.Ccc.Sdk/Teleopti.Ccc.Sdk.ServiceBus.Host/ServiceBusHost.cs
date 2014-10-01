using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.ServiceProcess;
using System.Threading;

namespace Teleopti.Ccc.Sdk.ServiceBus.Host
{
	public partial class ServiceBusHost : ServiceBase
	{
		private ServiceBusRunner serviceBusRunner;

		public ServiceBusHost()
		{
			InitializeComponent();
			serviceBusRunner = new ServiceBusRunner(RequestAdditionalTime);
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
				preserveStackTrace(exception);
				throw exception;
			}).Start();
		}

		// http://stackoverflow.com/questions/57383/in-c-how-can-i-rethrow-innerexception-without-losing-stack-trace
		private static void preserveStackTrace(Exception e)
		{
			var ctx = new StreamingContext(StreamingContextStates.CrossAppDomain);
			var mgr = new ObjectManager(null, ctx);
			var si = new SerializationInfo(e.GetType(), new FormatterConverter());

			e.GetObjectData(si, ctx);
			mgr.RegisterObject(e, 1, si); // prepare for SetObjectData
			mgr.DoFixups(); // ObjectManager calls SetObjectData

			// voila, e is unmodified save for _remoteStackTraceString
		}

		protected override void OnStop()
		{
			serviceBusRunner.Stop();
		}
	}
}
