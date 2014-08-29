using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.Client.SignalR.Wrappers;

namespace Teleopti.Messaging.Client.SignalR
{
	public class SignalRClient : ISignalRClient
	{
		private readonly IEnumerable<IConnectionKeepAliveStrategy> _connectionKeepAliveStrategy;
		private readonly ITime _time;
		private IHandleHubConnection _connection;
		private IStateAccessor _stateAccessor;
		private readonly ILog _logger = LogManager.GetLogger(typeof(SignalRClient));
		private Action<Notification> _onNotification = n => { };
		private Action _afterConnectionCreated = () => { };

		public SignalRClient(string serverUrl, IEnumerable<IConnectionKeepAliveStrategy> connectionKeepAliveStrategy, ITime time)
		{
			ServerUrl = serverUrl;

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
			if (string.IsNullOrEmpty(ServerUrl))
				return;

			var connection = new SignalRConnection(
				MakeHubConnection,
				_afterConnectionCreated,
				_connectionKeepAliveStrategy,
				_time);

			_connection = connection;
			_stateAccessor = connection;

			_connection.StartConnection(_onNotification, useLongPolling);
		}

		public void Call(string methodName, params object[] args)
		{
			_logger.Debug(methodName);
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

		public void RegisterCallbacks(Action<Notification> onNotification, Action afterConnectionCreated)
		{
			_onNotification = onNotification;
			_afterConnectionCreated = afterConnectionCreated;
		}

		public bool IsAlive
		{
			get { return _connection != null && _connection.IsConnected(); }
		}

		public string ServerUrl { get; set; }

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
			return new HubConnectionWrapper(new HubConnection(ServerUrl));
		}

	}
}