using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.Exceptions;
using log4net;
using Teleopti.Messaging.SignalR.Wrappers;

namespace Teleopti.Messaging.SignalR
{
	[CLSCompliant(false)]
	public class SignalConnection : IHandleHubConnection, ICallHubProxy
	{

		public const string ConnectionRestartedErrorMessage = "Connection closed. Trying to restart...";
		public const string ConnectionReconnected = "Connection reconnected successfully";

		private IHubProxyWrapper _hubProxy;
		private IHubConnectionWrapper _hubConnection;
		private readonly Func<IHubConnectionWrapper> _hubConnectionFactory;
		private readonly TimeSpan _restartDelay;

		protected ILog Logger;
		private readonly int _maxRestartAttempts;
		private readonly IRecreateConnectionStrategy _recreateConnectionStrategy;
		private readonly ITime _time;
		private int _reconnectAttempts;

		public SignalConnection(
			Func<IHubConnectionWrapper> hubConnectionFactory, 
			ILog logger,
			TimeSpan restartDelay, 
			IRecreateConnectionStrategy recreateConnectionStrategy,
			ITime time,
			int maxRestartAttempts = 0)
		{
			_hubConnectionFactory = hubConnectionFactory;
			
			_restartDelay = restartDelay;
			_maxRestartAttempts = maxRestartAttempts;
			_recreateConnectionStrategy = recreateConnectionStrategy;
			_time = time;

			Logger = logger ?? LogManager.GetLogger(typeof(SignalConnection));

		}

		private void createConnectionAndProxy()
		{
			_hubConnection = _hubConnectionFactory.Invoke();
			_hubProxy = _hubConnection.CreateHubProxy("MessageBrokerHub");
		}

		public void StartConnection(Action<Notification> onNotification)
		{
			createConnectionAndProxy();

			_recreateConnectionStrategy.Initialize(this, _time, createConnectionAndProxy);

			if (onNotification != null)
			{
				_hubProxy.Subscribe("OnEventMessage").Received += obj =>
				{
					var d = obj[0].ToObject<Notification>();
					onNotification(d);
				};
			}
			
			_hubConnection.Credentials = CredentialCache.DefaultNetworkCredentials;
			_hubConnection.Closed += restart;
			_hubConnection.Reconnected += hubConnectionOnReconnected;

			try
			{
				tryStartConnection();
			}
			catch (AggregateException aggregateException)
			{
				Logger.Error("An error happened when starting hub connection.", aggregateException);
				throw new BrokerNotInstantiatedException("Could not start the SignalR message broker.", aggregateException);
			}
			catch (SocketException socketException)
			{
				Logger.Error("An error happened when starting hub connection.", socketException);
				throw new BrokerNotInstantiatedException("Could not start the SignalR message broker.", socketException);
			}
			catch (InvalidOperationException exception)
			{
				Logger.Error("An error happened when starting hub connection.", exception);
				throw new BrokerNotInstantiatedException("Could not start the SignalR message broker.", exception);
			}
		}

		private void tryStartConnection()
		{
			Exception exception = null;
			var startTask = _hubConnection.Start();
			startTask.ContinueWith(t =>
				{
					if (t.IsFaulted && t.Exception != null)
					{
						exception = t.Exception.GetBaseException();
						Logger.Error("An error happened when starting hub connection.", exception);
					}
				}, TaskContinuationOptions.OnlyOnFaulted);

			if (!startTask.Wait(TimeSpan.FromSeconds(30)))
			{
				exception = new InvalidOperationException("Could not start message broker client within 30 seconds.");
			}
			if (exception != null)
			{
				throw exception;
			}
		}

		private void restart()
		{
			// ErikS: 2014-02-17
			// To handle lots of MyTime and MyTimeWeb clients we dont want to flood with restart attempts
			// Closed event triggers when the broker is actually down, IE the server died not from recycles
			if(_maxRestartAttempts == 0 || _reconnectAttempts < _maxRestartAttempts)
			{
				TaskHelper.Delay(_restartDelay).Wait();
				Logger.Error(ConnectionRestartedErrorMessage);
				_hubConnection.Start();
				_reconnectAttempts++;
			}
		}

		private void hubConnectionOnReconnected()
		{
			Logger.Info(ConnectionReconnected);
		}

		public void CloseConnection()
		{
			if (_hubConnection == null) return;

			_hubConnection.Reconnected -= hubConnectionOnReconnected;
			_hubConnection.Closed -= restart;

			try
			{
				if (_hubConnection.State == ConnectionState.Connected)
					_hubConnection.Stop();
			}
			catch (Exception ex)
			{
				Logger.Error("An error happened when stopping connection.", ex);
			}
		}

		public void WithProxy(Action<IHubProxyWrapper> action)
		{
			action.Invoke(_hubProxy);
		}

		public void IfProxyConnected(Action<IHubProxyWrapper> action)
		{
			if (_hubConnection.State == ConnectionState.Connected)
				action.Invoke(_hubProxy);
		}

		public bool IsConnected()
		{
			return _hubProxy != null && _hubConnection.State == ConnectionState.Connected;
		}
	}
}