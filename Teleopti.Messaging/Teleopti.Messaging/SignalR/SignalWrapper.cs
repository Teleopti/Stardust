using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Newtonsoft.Json.Linq;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.Exceptions;
using log4net;
using Subscription = Teleopti.Interfaces.MessageBroker.Subscription;

namespace Teleopti.Messaging.SignalR
{
	internal class SignalSubscriber
	{
		private readonly IHubProxy _hubProxy;
		private const string eventName = "OnEventMessage";

		private static readonly ILog Logger = LogManager.GetLogger(typeof(SignalSubscriber));

		public event Action<Notification> OnNotification;

		public SignalSubscriber(IHubProxy hubProxy)
		{
			_hubProxy = hubProxy;
		}

		public void Start()
		{
			_hubProxy.Subscribe(eventName).Received += subscription_Data;
		}

		public void Stop()
		{
			try
			{
				_hubProxy.Subscribe(eventName).Received -= subscription_Data;
			}
			catch (Exception ex)
			{
				Logger.Error("An error happened when stopping connection.", ex);
			}
		}

		private void subscription_Data(IList<JToken> obj)
		{
			var handler = OnNotification;
			if (handler != null)
			{
				var d = obj[0].ToObject<Notification>();
				handler.BeginInvoke(d, onNotificationCallback, handler);
			}
		}

		private void onNotificationCallback(IAsyncResult ar)
		{
			((Action<Notification>)ar.AsyncState).EndInvoke(ar);
		}
	}

	internal class SignalWrapper : ISignalWrapper
	{
		private readonly IHubProxy _hubProxy;
		private readonly HubConnection _hubConnection;
		private int _retryCount;
		private bool _isRunning;

		private static readonly ILog Logger = LogManager.GetLogger(typeof(SignalWrapper));
		private static readonly object LockObject = new object();
		
		public SignalWrapper(IHubProxy hubProxy, HubConnection hubConnection)
		{
			_isRunning = false;
			_hubProxy = hubProxy;
			_hubConnection = hubConnection;
		}

		public Task NotifyClients(Notification notification)
		{
			if (verifyStillConnected())
			{
				try
				{
					var startTask = _hubProxy.Invoke("NotifyClients", notification);
					startTask.ContinueWith(t =>
					{
						if (t.IsFaulted && t.Exception != null)
						{
							Logger.Error("An error happened when notifying.", t.Exception.GetBaseException());
						}
					}, TaskContinuationOptions.OnlyOnFaulted);
					return startTask;
				}
				catch (InvalidOperationException exception)
				{
					Logger.Error("An error happened when notifying.", exception);
				}
			}
			return emptyTask();
		}

		public Task NotifyClients(IEnumerable<Notification> notifications)
		{
			if (verifyStillConnected())
			{
				try
				{
					var startTask = _hubProxy.Invoke("NotifyClientsMultiple", notifications);
					startTask.ContinueWith(t =>
					{
						if (t.IsFaulted && t.Exception != null)
						{
							Logger.Error("An error happened when notifying multiple.", t.Exception.GetBaseException());
						}
					}, TaskContinuationOptions.OnlyOnFaulted);
					return startTask;
				}
				catch (InvalidOperationException exception)
				{
					Logger.Error("An error happened when notifying multiple.", exception);
				}
			}
			return emptyTask();
		}

		private bool verifyStillConnected()
		{
			if (_retryCount > 3)
			{
				return false;
			}

			lock (LockObject)
			{
				if (_hubConnection.State != ConnectionState.Connected)
				{
					try
					{
						startHubConnection();
						_retryCount = 0;
						return true;
					}
					catch (Exception ex)
					{
						//Suppress! Already logged upon startup for general failures.
						Logger.Error("An error happened when verifying that we still are connected.", ex);
						_retryCount++;
						return false;
					}
				}
			}
			return true;
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
				if (exception!=null)
				{
					throw exception;
				}

				_isRunning = true;
				_hubConnection.Closed += () => { _isRunning = false; };
				_hubConnection.Reconnected += () => { _isRunning = true; };
				_hubConnection.Error += e => { _isRunning = false; };
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
			return emptyTask();
		}

		private static Task emptyTask()
		{
			var tcs = new TaskCompletionSource<object>();
			tcs.SetResult(null);
			return tcs.Task;
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
			return emptyTask();
		}

		public void StopHub()
		{
			if (_hubConnection != null && _hubConnection.State==ConnectionState.Connected)
			{
				try
				{
					_hubConnection.Stop();
					_isRunning = false;
				}
				catch (Exception ex)
				{
					Logger.Error("An error happened when stopping connection.", ex);
				}
			}
		}

		public bool IsInitialized()
		{
			return _hubProxy != null && _isRunning;
		}
	}
}