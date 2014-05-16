using System;
using System.Collections.Generic;
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
	public class SignalConnection : IHandleHubConnection, IStateAccessor
	{
		private IHubProxyWrapper _hubProxy;
		private IHubConnectionWrapper _hubConnection;
		private readonly Func<IHubConnectionWrapper> _hubConnectionFactory;
		private readonly Action _afterConnectionCreated;

		private readonly ILog _logger = LogManager.GetLogger(typeof(SignalConnection));
		private readonly IEnumerable<IConnectionKeepAliveStrategy> _connectionKeepAliveStrategy;
		private readonly ITime _time;
		private Action<Notification> _onNotification;

		public SignalConnection(
			Func<IHubConnectionWrapper> hubConnectionFactory, 
			Action afterConnectionCreated,
			IEnumerable<IConnectionKeepAliveStrategy> connectionKeepAliveStrategy,
			ITime time
			)
		{
			_hubConnectionFactory = hubConnectionFactory;
			_afterConnectionCreated = afterConnectionCreated;
			_connectionKeepAliveStrategy = connectionKeepAliveStrategy;
			_time = time;
		}

		private void createConnectionAndProxy()
		{
			_hubConnection = _hubConnectionFactory.Invoke();
			_hubConnection.Credentials = CredentialCache.DefaultNetworkCredentials;
			_hubConnection.Reconnected += () => _logger.Info("Connection reconnected successfully");
			_hubProxy = _hubConnection.CreateHubProxy("MessageBrokerHub");

			foreach (var strategy in _connectionKeepAliveStrategy)
				strategy.OnNewConnection(this);

			_hubProxy.Subscribe("OnEventMessage").Received += obj =>
			{
				var d = obj[0].ToObject<Notification>();
				if (_onNotification != null)
					_onNotification(d);
			};

			_afterConnectionCreated();
		}

		public void StartConnection(Action<Notification> onNotification)
		{
			_onNotification = onNotification;

			createConnectionAndProxy();

			foreach (var strategy in _connectionKeepAliveStrategy)
				strategy.OnStart(this, _time, createConnectionAndProxy);

			try
			{
				Exception exception = null;
				var startTask = _hubConnection.Start();
				startTask.ContinueWith(t =>
				{
					if (t.IsFaulted && t.Exception != null)
					{
						exception = t.Exception.GetBaseException();
						_logger.Error("An error happened when starting hub connection.", exception);
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
			catch (AggregateException aggregateException)
			{
				_logger.Error("An error happened when starting hub connection.", aggregateException);
				throw new BrokerNotInstantiatedException("Could not start the SignalR message broker.", aggregateException);
			}
			catch (SocketException socketException)
			{
				_logger.Error("An error happened when starting hub connection.", socketException);
				throw new BrokerNotInstantiatedException("Could not start the SignalR message broker.", socketException);
			}
			catch (InvalidOperationException exception)
			{
				_logger.Error("An error happened when starting hub connection.", exception);
				throw new BrokerNotInstantiatedException("Could not start the SignalR message broker.", exception);
			}
		}

		public void CloseConnection()
		{
			try
			{
				foreach (var strategy in _connectionKeepAliveStrategy)
					strategy.OnClose(this);

				if (_hubConnection.State == ConnectionState.Connected)
					_hubConnection.Stop();
			}
			catch (Exception ex)
			{
				_logger.Error("An error happened when stopping connection.", ex);
			}
		}

		public void WithConnection(Action<IHubConnectionWrapper> action)
		{
			action.Invoke(_hubConnection);
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