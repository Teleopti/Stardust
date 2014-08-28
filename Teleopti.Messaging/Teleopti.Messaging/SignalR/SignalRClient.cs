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
	public class SignalRSender : IMessageSender
	{
		private readonly ISignalRClient _client;

		public SignalRSender(ISignalRClient client)
		{
			_client = client;
		}

		public void SendNotification(Notification notification)
		{
			_client.Call("NotifyClients", notification);
		}
	}

	public static class MessageBrokerContainer
	{
		private static ISignalRClient _client;
		private static IMessageSender _sender;

		public static void Initialize(string serverUrl)
		{
			_client = new SignalRClient(serverUrl, new IConnectionKeepAliveStrategy[] { }, new Time(new Now()));
			_sender = new SignalRSender(_client);
		}

		public static ISignalRClient SignalRClient()
		{
			return _client;
		}

		public static IMessageSender Sender()
		{
			return _sender;
		}
	}

	public class SignalRClient : ISignalRClient
	{
		private readonly IEnumerable<IConnectionKeepAliveStrategy> _connectionKeepAliveStrategy;
		private readonly ITime _time;
		private readonly string _serverUrl;
		private IHandleHubConnection _connection;
		private IStateAccessor _stateAccessor;
		private readonly ILog _logger = LogManager.GetLogger(typeof(SignalRClient));

		public SignalRClient(string serverUrl, IEnumerable<IConnectionKeepAliveStrategy> connectionKeepAliveStrategy, ITime time)
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

		public void StartBrokerService(bool useLongPolling = false)
		{
			if (string.IsNullOrEmpty(_serverUrl))
				return;
			try
			{
				if (_connection == null)
				{
					var connection = new SignalConnection(MakeHubConnection, () => { }, _connectionKeepAliveStrategy, _time);
					
					_connection = connection;
					_stateAccessor = connection;
				}

				_connection.StartConnection(null,useLongPolling);
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

		public void Call(string methodName, params object[] args)
		{
			if (_stateAccessor == null)
				return;
			_stateAccessor.IfProxyConnected(p =>
			{
				var task = p.Invoke(methodName, args);

				task.ContinueWith(t =>
				{
					if (t.IsFaulted && t.Exception != null)
						_logger.Info("An error occurred on task calling " + methodName, t.Exception);
				}, TaskContinuationOptions.OnlyOnFaulted);
			});
		}

		public bool IsAlive
		{
			get { return _connection != null && _connection.IsConnected(); }
		}

		public virtual void Dispose()
		{
			_connection.CloseConnection();
			_connection = null;
			_stateAccessor = null;
		}

		[CLSCompliant(false)]
		protected virtual IHubConnectionWrapper MakeHubConnection()
		{
			return new HubConnectionWrapper(new HubConnection(_serverUrl));
		}

	}
}