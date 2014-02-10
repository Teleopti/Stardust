using System;
using System.Net;
using System.Threading.Tasks;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using log4net;

namespace Teleopti.Messaging.SignalR
{
	public class SignalSender : SignalSenderBase, IMessageSender
	{
		[CLSCompliant(false)]
		protected ILog Logger;

		public SignalSender(string serverUrl)
		{
			// ReSharper disable DoNotCallOverridableMethodsInConstructor
			Logger = MakeLogger();
			// ReSharper restore DoNotCallOverridableMethodsInConstructor
			_serverUrl = serverUrl;

			ServicePointManager.ServerCertificateValidationCallback = ignoreInvalidCertificate;
			ServicePointManager.DefaultConnectionLimit = 50;

			TaskScheduler.UnobservedTaskException += taskSchedulerOnUnobservedTaskException;

		}

		public void SendNotification(Notification notification)
		{
			try
			{
				var task = _wrapper.NotifyClients(notification);
				task.Wait(1000);
			}
			catch (AggregateException e)
			{
				Logger.Error("Could not send notifications, ", e);
			}
		}


		public void Dispose()
		{
			_wrapper.StopHub();
			_wrapper = null;
		}

		[CLSCompliant(false)]
		protected virtual ILog MakeLogger()
		{
			return LogManager.GetLogger(typeof(SignalSender));
		}
	}
}