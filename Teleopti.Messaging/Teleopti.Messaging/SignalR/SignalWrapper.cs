using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SignalR.Client;
using SignalR.Client.Hubs;
using SignalR.Client.Transports;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.Exceptions;
using Subscription = Teleopti.Interfaces.MessageBroker.Subscription;

namespace Teleopti.Messaging.SignalR
{
	internal class SignalWrapper
	{
		private readonly IHubProxy _hubProxy;
		private readonly HubConnection _hubConnection;
		private const string EventName = "OnEventMessage";
		private int _retryCount = 0;
		private static readonly object LockObject = new object();
		private bool _isRunning = false;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event Action<Notification> OnNotification;

		public SignalWrapper(IHubProxy hubProxy, HubConnection hubConnection)
		{
			_hubProxy = hubProxy;
			_hubConnection = hubConnection;
		}

		public Task NotifyClients(Notification notification)
		{
			if (verifyStillConnected())
			{
				return _hubProxy.Invoke("NotifyClients", notification);
			}
			return emptyTask();
		}

		public Task NotifyClients(IEnumerable<Notification> notifications)
		{
			if (verifyStillConnected())
			{
				return _hubProxy.Invoke("NotifyClientsMultiple", notifications);
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
					catch (Exception)
					{
						//Suppress! Already logged upon startup for general failures.
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
				_hubConnection.StateChanged += s => { _isRunning = s.NewState == ConnectionState.Connected; };
			}
			catch (AggregateException aggregateException)
			{
				throw new BrokerNotInstantiatedException("Could not start the SignalR message broker.", aggregateException);
			}
			catch (SocketException socketException)
			{
				throw new BrokerNotInstantiatedException("Could not start the SignalR message broker.", socketException);
			}
			catch (InvalidOperationException exception)
			{
				throw new BrokerNotInstantiatedException("Could not start the SignalR message broker.", exception);
			}
		}

		private void subscription_Data(object[] obj)
		{
			var handler = OnNotification;
			if (handler!=null)
			{
				var d = ((JObject)obj[0]).ToObject<Notification>();
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
				return _hubProxy.Invoke("AddSubscription", subscription);
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
						 t.Exception.GetBaseException();
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
					var proxy = (HubProxy)_hubProxy;
					var subscriptionList = new List<string>(proxy.GetSubscriptions());
					if (subscriptionList.Contains(EventName))
					{
						_hubProxy.Subscribe(EventName).Data -= subscription_Data;
					}

					_hubConnection.Stop();
					_isRunning = false;
				}
				catch (Exception)
				{
				}
			}
		}

		public bool IsInitialized()
		{
			return _hubProxy != null && _isRunning;
		}
	}
}