using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Newtonsoft.Json.Linq;
using Teleopti.Interfaces.MessageBroker;
using log4net;

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
}