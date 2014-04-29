using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.Exceptions;
using log4net;
using Subscription = Teleopti.Interfaces.MessageBroker.Subscription;

namespace Teleopti.Messaging.SignalR
{
	[CLSCompliant(false)]
	public class SignalConnectionHandler : ISignalConnectionHandler
	{
		private const string notifyclients = "NotifyClients";
		private const string notifyclientsmultiple = "NotifyClientsMultiple";
		private const string addsubscription = "AddSubscription";
		private const string removesubscription = "RemoveSubscription";

		public const string ConnectionRestartedErrorMessage = "Connection closed. Trying to reconnect...";
		public const string ConnectionReconnected = "Connection reconnected successfully";

		private readonly IHubProxy _hubProxy;
		private readonly IHubConnectionWrapper _hubConnection;
		private readonly TimeSpan _reconnectDelay;
		private readonly Task emptyTask;

		protected ILog Logger ;
		private int reconnectAttempts;

		public SignalConnectionHandler(IHubProxy hubProxy, IHubConnectionWrapper hubConnection, ILog logger, TimeSpan reconnectDelay)
		{
			_hubProxy = hubProxy;
			_hubConnection = hubConnection;
			_reconnectDelay = reconnectDelay;

			Logger = logger ?? LogManager.GetLogger(typeof(SignalConnectionHandler));

			emptyTask = TaskHelper.MakeEmptyTask();
		}
		
		public void StartConnection()
		{
			_hubConnection.Credentials = CredentialCache.DefaultNetworkCredentials;
			_hubConnection.Closed += reconnect;
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

		private void reconnect()
		{
			// ErikS: 2014-02-17
			// To handle lots of MyTime and MyTimeWeb clients we dont want to flood with reconnect attempts
			// Closed event triggers when the broker is actually down, IE the server died not from recycles
			TaskHelper.Delay(_reconnectDelay).Wait();
			Logger.Error(ConnectionRestartedErrorMessage);
			_hubConnection.Start();
		}

		private void hubConnectionOnReconnected()
		{
			Logger.Info(ConnectionReconnected);
		}

		public void CloseConnection()
		{
			if (_hubConnection == null) return;

			_hubConnection.Reconnected -= hubConnectionOnReconnected;
			_hubConnection.Closed -= reconnect;

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

		public Task AddSubscription(Subscription subscription)
		{
			return notify(addsubscription, subscription);
		}

		public Task RemoveSubscription(string route)
		{
			return notify(removesubscription, route);
		}

		public Task NotifyClients(Notification notification)
		{
			return notify(notifyclients, notification);
		}

		public Task NotifyClients(IEnumerable<Notification> notifications)
		{
			return notify(notifyclientsmultiple, notifications);
		}

		private Task notify(string methodName, params object[] notifications)
		{
			if (_hubConnection.State == ConnectionState.Connected)
			{
				var task = _hubProxy.Invoke(methodName, notifications);

				return task.ContinueWith(t =>
					{
						if (t.IsFaulted && t.Exception != null)
							Logger.Debug("An error happened on notification task", t.Exception);
					}, TaskContinuationOptions.OnlyOnFaulted);
			}
			return emptyTask;
		}

		public bool IsInitialized()
		{
			return _hubProxy != null && _hubConnection.State == ConnectionState.Connected;
		}
	}
}