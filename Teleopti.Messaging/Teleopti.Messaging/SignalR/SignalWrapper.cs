using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json.Linq;
using SignalR.Client;
using SignalR.Client.Hubs;
using SignalR.Client.Net20.Infrastructure;
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
		private bool _isRunning;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event Action<Notification> OnNotification;

		public SignalWrapper(IHubProxy hubProxy, HubConnection hubConnection)
		{
			_hubProxy = hubProxy;
			_hubConnection = hubConnection;
		}

		public Task<object> NotifyClients(Notification notification)
		{
			if (verifyStillConnected())
			{
				return _hubProxy.Invoke("NotifyClients", notification);
			}
			return emptyTask();
		}

		public Task<object> NotifyClients(IEnumerable<Notification> notifications)
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
			var subscription = _hubProxy.Subscribe(EventName);
			subscription.Data += subscription_Data;

			startHubConnection();
			_hubProxy.Subscribe(EventName);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SignalR")]
		private void startHubConnection()
		{
			try
			{
				var resetEvent = new ManualResetEvent(false);
				Exception startException = null;
				_hubConnection.Start(new LongPollingTransport()).ContinueWith(t =>
				{
					if (t.IsFaulted)
					{
						startException = t.Exception;
					}
					resetEvent.Set();
				});
				if (resetEvent.WaitOne()==false)
				{
					throw new InvalidOperationException("Time out occurred upon startup of Message Broker.");
				}
				if (startException!=null)
				{
					throw startException;
				}

				_isRunning = true;
				_hubConnection.Closed += () => { _isRunning = false; };
				_hubConnection.Reconnected += () => { _isRunning = true; };
			}
			catch (InvalidOperationException exception)
			{
				throw new BrokerNotInstantiatedException("Could not start the SignalR message broker.",exception);
			}
		}

		private void subscription_Data(object[] obj)
		{
			var handler = OnNotification;
			if (handler!=null)
			{
				var d = ((JObject)obj[0]).ToObject<Notification>();
				handler.BeginInvoke(d,onNotificationCallback,handler);
			}
		}

		private void onNotificationCallback(IAsyncResult ar)
		{
			((Action<Notification>)ar.AsyncState).EndInvoke(ar);
		}

		public Task<object> AddSubscription(Subscription subscription)
		{
			if (verifyStillConnected())
			{
				return _hubProxy.Invoke("AddSubscription", subscription);
			}
			return emptyTask();
		}

		private static Task<object> emptyTask()
		{
			var task = new Task<object>();
			task.OnFinished(null, null);
			return task;
		}

		public Task<object> RemoveSubscription(string route)
		{
			if (verifyStillConnected())
			{
				return _hubProxy.Invoke("RemoveSubscription", route);
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
						var subscription = _hubProxy.Subscribe(EventName);
						subscription.Data -= subscription_Data;
					}

					ThreadPool.QueueUserWorkItem(state =>
					                             	{
					                             		try
					                             		{
															_hubConnection.Stop();
					                             		}
					                             		catch (Exception)
					                             		{
					                             		}
					                             	});

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