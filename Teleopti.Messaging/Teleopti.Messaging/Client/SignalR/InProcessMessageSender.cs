using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Teleopti.Ccc.Domain.Config;
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
		private readonly int _messagesPerSecond;
		private bool isRunning;
		private readonly object lockObject = new object();

		public InProcessMessageSender(IMessageBrokerServer messageBrokerServer, IConfigReader config)
		{
			_messageBrokerServer = messageBrokerServer;
			_actions = new ConcurrentQueue<Action>();
			_messagesPerSecond = config.ReadValue("MessagesPerSecond", 80);
		}

		public void Send(Message message)
		{
			_actions.Enqueue(() => _messageBrokerServer.NotifyClients(message));
			ensureProcessing();
		}

		public void SendMultiple(IEnumerable<Message> messages)
		{
			_actions.Enqueue(() => _messageBrokerServer.NotifyClientsMultiple(messages));
			ensureProcessing();
		}

		private void ensureProcessing()
		{
			lock (lockObject)
			{
				if (isRunning) return;
				isRunning = true;
				Task.Run(() => processQueue());
			}
		}

		private void processQueue()
		{
			while (!_actions.IsEmpty)
			{
				Action action;
				if (!_actions.TryDequeue(out action)) continue;
				try
				{
					action();
				}
				catch (Exception e)
				{
					logger.Error("An error occurred while sending messages in process.", e);
				}
				Thread.Sleep(1000 / _messagesPerSecond);
			}
			lock (lockObject)
			{
				isRunning = false;
			}
		}
	}
}