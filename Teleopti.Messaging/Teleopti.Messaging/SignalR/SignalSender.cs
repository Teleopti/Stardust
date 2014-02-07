using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Exceptions;
using log4net;
using Subscription = Teleopti.Interfaces.MessageBroker.Subscription;

namespace Teleopti.Messaging.SignalR
{
	public class SignalSender : IMessageSender
	{
		private readonly string _serverUrl;
		private ISignalWrapper _wrapper;

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

		private static bool ignoreInvalidCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
		{
			return true;
		}

		private void taskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			if (e.Observed) return;
			Logger.Error("An error occured, please review the error and take actions necessary.", e.Exception);
			e.SetObserved();
		}

		public void StartBrokerService()
		{
			try
			{
				var connection = MakeHubConnection();
				var proxy = connection.CreateHubProxy("MessageBrokerHub");

				_wrapper = _wrapper ?? new SignalWrapper(proxy, connection, Logger);
				_wrapper.StartHub();
			}
			catch (SocketException exception)
			{
				Logger.Error("The message broker seems to be down.", exception);
			}
			catch (BrokerNotInstantiatedException exception)
			{
				Logger.Error("The message broker seems to be down.", exception);
			}
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
		
		public bool IsAlive
		{
			get { return _wrapper != null && _wrapper.IsInitialized(); }
		}

		public void Dispose()
		{
			_wrapper.StopHub();
			_wrapper = null;
		}


		[CLSCompliant(false)]
		protected virtual IHubConnectionWrapper MakeHubConnection()
		{
			return new HubConnectionWrapper(new HubConnection(_serverUrl));
		}

		[CLSCompliant(false)]
		protected virtual ILog MakeLogger()
		{
			return LogManager.GetLogger(typeof(AsyncSignalSender));
		}
	}
}