using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Server;

namespace Teleopti.Messaging.Client.SignalR
{
	public class InProcessMessageSender : IMessageSender
	{
		private readonly IMessageBrokerServer _messageBrokerServer;
		private readonly ConcurrentQueue<Action> _actions;
		private readonly ILog logger = LogManager.GetLogger(typeof(InProcessMessageSender));

		public InProcessMessageSender(IMessageBrokerServer messageBrokerServer)
		{
			_messageBrokerServer = messageBrokerServer;
			_actions = new ConcurrentQueue<Action>();
			Task.Run(() => processQueue());
		}

		public void Send(Message message)
		{
			_actions.Enqueue(() => _messageBrokerServer.NotifyClients(message));
		}

		public void SendMultiple(IEnumerable<Message> messages)
		{
			_actions.Enqueue(() => _messageBrokerServer.NotifyClientsMultiple(messages));
		}

		private void processQueue()
		{
			while (true)
			{
				if (_actions.IsEmpty)
				{
					Thread.Sleep(TimeSpan.FromSeconds(1));
					continue;
				}
				Action action;
				if (_actions.TryDequeue(out action))
				{
					try
					{
						action();
					}
					catch (Exception e)
					{
						logger.Error("An error occured while sending messages in process.", e);
					}
				}

			}
		}
	}
}