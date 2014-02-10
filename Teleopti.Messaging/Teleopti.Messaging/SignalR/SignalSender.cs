using System;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using log4net;

namespace Teleopti.Messaging.SignalR
{
	public class SignalSender : SignalSenderBase, IMessageSender
	{
		public SignalSender(string serverUrl) : base(serverUrl)
		{
			Logger = MakeLogger();
		}

		public void SendNotification(Notification notification)
		{
			try
			{
				var task = Wrapper.NotifyClients(notification);
				task.Wait(1000);
			}
			catch (AggregateException e)
			{
				Logger.Error("Could not send notifications, ", e);
			}
		}

		[CLSCompliant(false)]
		protected virtual ILog MakeLogger()
		{
			return LogManager.GetLogger(typeof(SignalSender));
		}
	}
}