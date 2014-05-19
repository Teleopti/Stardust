using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.Exceptions;
using log4net;
using Teleopti.Messaging.SignalR.Wrappers;

namespace Teleopti.Messaging.SignalR
{
	public class SignalSender : IMessageSender
	{
		private readonly IEnumerable<IConnectionKeepAliveStrategy> _connectionKeepAliveStrategy;
		private readonly ITime _time;
		private readonly string _serverUrl;
		private IHandleHubConnection _connection;
		private ISignalBrokerCommands _signalBrokerCommands;
		private readonly ILog _logger = LogManager.GetLogger(typeof(SignalSender));

		public static SignalSender MakeForEtl(string serverUrl)
		{
			return new SignalSender(serverUrl, new IConnectionKeepAliveStrategy[] { }, new Time(new Now()));
		}

		public SignalSender(string serverUrl, IEnumerable<IConnectionKeepAliveStrategy> connectionKeepAliveStrategy, ITime time)
		{
			_serverUrl = serverUrl;

			ServicePointManager.ServerCertificateValidationCallback = IgnoreInvalidCertificate;
			ServicePointManager.DefaultConnectionLimit = 50;

			TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

			_connectionKeepAliveStrategy = connectionKeepAliveStrategy;
			_time = time;
		}

		protected static bool IgnoreInvalidCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
		{
			return true;
		}

		protected void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			if (e.Observed) return;
			_logger.Debug("An unobserved task failed.", e.Exception);
			e.SetObserved();
		}

		public void StartBrokerService()
		{
			if (string.IsNullOrEmpty(_serverUrl))
				return;
			try
			{
				if (_connection == null)
				{
					var connection = new SignalConnection(MakeHubConnection, () => { }, _connectionKeepAliveStrategy, _time);
					
					_signalBrokerCommands = new SignalBrokerCommands(_logger, connection);
					_connection = connection;
				}

				_connection.StartConnection(null);
			}
			catch (SocketException exception)
			{
				_logger.Error("The message broker seems to be down.", exception);
			}
			catch (BrokerNotInstantiatedException exception)
			{
				_logger.Error("The message broker seems to be down.", exception);
			}
		}

		public bool IsAlive
		{
			get { return _connection != null && _connection.IsConnected(); }
		}

		public void SendNotification(Notification notification)
		{
			_signalBrokerCommands.NotifyClients(notification);
		}

		public virtual void Dispose()
		{
			_connection.CloseConnection();
			_connection = null;
		}

		[CLSCompliant(false)]
		protected virtual IHubConnectionWrapper MakeHubConnection()
		{
			return new HubConnectionWrapper(new HubConnection(_serverUrl));
		}

	}
}