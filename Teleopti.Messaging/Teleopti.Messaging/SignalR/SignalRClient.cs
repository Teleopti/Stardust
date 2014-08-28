using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Core;
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
		private static string _serverUrl;
		private static IMessageFilterManager _messageFilter;
		private static ISignalRClient _client;
		private static IMessageSender _sender;
		private static IMessageBroker _compositeClient;

		public static void Configure(string serverUrl, IMessageFilterManager messageFilter)
		{
			_serverUrl = serverUrl;
			_messageFilter = messageFilter;
		}

		private static IMessageFilterManager messageFilter()
		{
			return _messageFilter ?? MessageFilterManager.Instance;
		}

		public static ISignalRClient SignalRClient()
		{
			return _client ?? (_client = new SignalRClient(_serverUrl, new IConnectionKeepAliveStrategy[] {}, new Time(new Now())));
		}

		public static IMessageBroker CompositeClient()
		{
			if (_compositeClient == null)
				_compositeClient = new SignalBroker(_serverUrl, messageFilter(), new IConnectionKeepAliveStrategy[] { new RestartOnClosed(), new RecreateOnNoPingReply() }, new Time(new Now()))
				{
					ConnectionString = _serverUrl
				};
			return _compositeClient;
		}

		public static IMessageSender Sender()
		{
			return _sender ?? (_sender = new SignalRSender(SignalRClient()));
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

			var connection = new SignalConnection(
				MakeHubConnection,
				() => { },
				_connectionKeepAliveStrategy,
				_time);

			_connection = connection;
			_stateAccessor = connection;

			_connection.StartConnection(null, useLongPolling);
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
			if (_connection == null) return;
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