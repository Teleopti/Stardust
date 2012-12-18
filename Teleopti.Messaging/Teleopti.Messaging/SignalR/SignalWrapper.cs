﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Microsoft.AspNet.SignalR.Client.Transports;
using Newtonsoft.Json.Linq;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.Exceptions;
using log4net;
using Subscription = Teleopti.Interfaces.MessageBroker.Subscription;

namespace Teleopti.Messaging.SignalR
{
	internal class SignalWrapper
	{
		private readonly IHubProxy _hubProxy;
		private readonly HubConnection _hubConnection;
		private const string EventName = "OnEventMessage";
		private int _retryCount;
		private static readonly object LockObject = new object();
		private bool _isRunning;
		private static readonly ILog Logger = LogManager.GetLogger(typeof (SignalWrapper));

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event Action<Notification> OnNotification;

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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
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

		public void StartListening()
		{
			_hubProxy.Subscribe(EventName).Data += subscription_Data;

			startHubConnection();
			_hubProxy.Subscribe(EventName);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SignalR")]
		private void startHubConnection()
		{
			try
			{
				Exception exception = null;
				var startTask = _hubConnection.Start(new LongPollingTransport());
				startTask.ContinueWith(t =>
				                       	{
				                       		if (t.IsFaulted && t.Exception != null)
				                       		{
												exception = t.Exception.GetBaseException();
												Logger.Error("An error happened when starting hub connection.", exception);
				                       		}
				                       	}, TaskContinuationOptions.OnlyOnFaulted);
				
				if (!startTask.Wait(TimeSpan.FromSeconds(10)))
				{
					exception = new InvalidOperationException("Could not start within given time limit.");
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

		private void subscription_Data(JToken[] obj)
		{
			var handler = OnNotification;
			if (handler!=null)
			{
				var d = obj[0].ToObject<Notification>();
				handler.BeginInvoke(d, onNotificationCallback,handler);
			}
		}

		private void onNotificationCallback(IAsyncResult ar)
		{
			((Action<Notification>)ar.AsyncState).EndInvoke(ar);
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework", MessageId = "System.Threading.WaitHandle.#WaitOne(System.Int32)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public void StopListening()
		{
			if (_hubConnection != null && _hubConnection.State==ConnectionState.Connected)
			{
				try
				{
					_hubProxy.Subscribe(EventName).Data -= subscription_Data;

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