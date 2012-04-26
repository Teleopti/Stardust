using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json.Linq;
using SignalR.Client._20.Hubs;
using SignalR.Client._20.Transports;
using Teleopti.Interfaces.MessageBroker;
using Subscription = Teleopti.Interfaces.MessageBroker.Subscription;

namespace Teleopti.Messaging.SignalR
{
	public class SignalWrapper
	{
		private readonly IHubProxy _hubProxy;
		private readonly HubConnection _hubConnection;
		private const string EventName = "OnEventMessage";
		private static readonly object LockObject = new object();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event Action<Notification> OnNotification;

		public SignalWrapper(IHubProxy hubProxy, HubConnection hubConnection)
		{
			_hubProxy = hubProxy;
			_hubConnection = hubConnection;
		}

		public EventSignal<object> NotifyClients(Notification notification)
		{
			verifyStillConnected();
			return _hubProxy.Invoke("NotifyClients", notification);
		}

		private void verifyStillConnected()
		{
			lock (LockObject)
			{
				if (!_hubConnection.IsActive)
				{
					_hubConnection.Start();
				}
			}
		}

		public void StartListening()
		{
			var subscription = _hubProxy.Subscribe(EventName);
			subscription.Data += subscription_Data;

			_hubConnection.Start();
			_hubProxy.Subscribe(EventName);
		}

		private void subscription_Data(object[] obj)
		{
			var handler = OnNotification;
			if (handler!=null)
			{
				var d = ((JObject)obj[0]).ToObject<Notification>();
				handler.Invoke(d);
			}
		}

		public EventSignal<object> AddSubscription(Subscription subscription)
		{
			verifyStillConnected();
			return _hubProxy.Invoke("AddSubscription", subscription);
		}

		public EventSignal<object> RemoveSubscription(string route)
		{
			verifyStillConnected();
			return _hubProxy.Invoke("RemoveSubscription", route);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework", MessageId = "System.Threading.WaitHandle.#WaitOne(System.Int32)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public void StopListening()
		{
			if (_hubConnection != null && _hubConnection.IsActive)
			{
				var reset = new ManualResetEvent(false);
				var signal = _hubProxy.Invoke("NotifyClients", new Notification());
				signal.Finished += (sender, e) =>
				                   	{
				                   		var proxy = (HubProxy)_hubProxy;
				                   		var subscriptionList = new List<string>(proxy.GetSubscriptions());
				                   		if (subscriptionList.Contains(EventName))
				                   		{
				                   			var subscription = _hubProxy.Subscribe(EventName);
				                   			subscription.Data -= subscription_Data;
				                   		}
				                   		_hubConnection.Stop();
				                   		reset.Set();
				                   	};
				try
				{
					reset.WaitOne(20 * 1000);
				}
				catch (Exception)
				{
				}
			}
		}

		public bool IsInitialized()
		{
			return _hubProxy != null;
		}
	}
}