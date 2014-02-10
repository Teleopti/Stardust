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
	public class SignalWrapper : ISignalWrapper
	{
		private const string notifyclients = "NotifyClients";
		private const string notifyclientsmultiple = "NotifyClientsMultiple";

		private readonly IHubProxy _hubProxy;
		private readonly IHubConnectionWrapper _hubConnection;

		protected ILog Logger ;
		private static readonly object LockObject = new object();
		private readonly Task emptyTask;
		
		public SignalWrapper(IHubProxy hubProxy, IHubConnectionWrapper hubConnection, ILog logger)
		{
			_hubProxy = hubProxy;
			_hubConnection = hubConnection;
			_hubConnection.Closed += restartConnection();

			Logger = logger ?? LogManager.GetLogger(typeof(SignalWrapper));
			
			var tcs = new TaskCompletionSource<object>();
			tcs.SetResult(null);
			emptyTask = tcs.Task;
		}

		private Action restartConnection()
		{
			return () => _hubConnection.Start();
		}

		public void StartHub()
		{
			startHubConnection();
		}

		private void startHubConnection()
		{
			try
			{
				Exception exception = null;
				_hubConnection.Credentials = CredentialCache.DefaultNetworkCredentials;
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

		public Task AddSubscription(Subscription subscription)
		{
			if (verifyStillConnected())
			{
				try
				{
					var startTask = _hubProxy.Invoke("AddSubscription", subscription);
					startTask.ContinueWith(t =>
					{
						if (t.IsFaulted && t.Exception != null)
						{
							Logger.Error("An error happened when adding subscription.", t.Exception.GetBaseException());
						}
					}, TaskContinuationOptions.OnlyOnFaulted);
					return startTask;
				}
				catch (InvalidOperationException exception)
				{
					Logger.Error("An error happened when adding subscriptions.", exception);
				}
			}
			return emptyTask;
		}

		public Task RemoveSubscription(string route)
		{
			if (verifyStillConnected())
			{
				var startTask = _hubProxy.Invoke("RemoveSubscription", route);
				startTask.ContinueWith(t =>
				{
					if (t.IsFaulted && t.Exception != null)
					{
						Logger.Error("An error happened when removing subscription.", t.Exception.GetBaseException());
					}
				}, TaskContinuationOptions.OnlyOnFaulted);
				return startTask;
			}
			return emptyTask;
		}

		private bool verifyStillConnected()
		{
			if (_hubConnection.State != ConnectionState.Connected)
			{
				lock (LockObject)
				{
					if (_hubConnection.State != ConnectionState.Connected)
					{
						try
						{
							startHubConnection();
							return true;
						}
						catch (Exception ex)
						{
							//Suppress! Already logged upon startup for general failures.
							Logger.Error("An error happened when verifying that we still are connected.", ex);
							return false;
						}
					}
				}
			}
			return true;
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
			try
			{
				if (_hubConnection.State == ConnectionState.Connected)
					return _hubProxy.Invoke(methodName, notifications);
			}
			catch (InvalidOperationException exception)
			{
				Logger.Error("An error happened when notifying multiple.", exception);
			}
			return emptyTask;
		}
		
		public void StopHub()
		{
			if (_hubConnection == null || _hubConnection.State != ConnectionState.Connected) return;
			
			try
			{
				_hubConnection.Closed -= restartConnection();
				_hubConnection.Stop();
			}
			catch (Exception ex)
			{
				Logger.Error("An error happened when stopping connection.", ex);
			}
		}

		public bool IsInitialized()
		{
			return _hubProxy != null;
		}
	}
}