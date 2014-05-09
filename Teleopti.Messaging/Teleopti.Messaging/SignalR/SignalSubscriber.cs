using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Teleopti.Interfaces.MessageBroker;
using log4net;

namespace Teleopti.Messaging.SignalR
{
	public interface ISignalSubscriber
	{
		event Action<Notification> OnNotification;
		void Start();
		void Stop();
	}

	public class SignalSubscriber : ISignalSubscriber
	{
		private readonly ICallHubProxy _hubProxy;
		private const string eventName = "OnEventMessage";

		private static readonly ILog Logger = LogManager.GetLogger(typeof(SignalSubscriber));

		public event Action<Notification> OnNotification;

		[CLSCompliant(false)]
		public SignalSubscriber(ICallHubProxy hubProxy)
		{
			_hubProxy = hubProxy;
		}

		public void Start()
		{
			_hubProxy.WithProxy(p =>
			{
				p.Subscribe(eventName).Received += subscription_Data;
			});
		}

		public void Stop()
		{
			try
			{
				_hubProxy.WithProxy(p =>
				{
					p.Subscribe(eventName).Received -= subscription_Data;
				});
			}
			catch (Exception ex)
			{
				Logger.Error("An error happened when stopping connection.", ex);
			}
		}

		private void subscription_Data(IList<JToken> obj)
		{
			if (OnNotification != null)
			{
				var d = obj[0].ToObject<Notification>();
				OnNotification(d);
			}
		}

	}

}