using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
		private bool isProcessingQueue;
		private readonly object processingLock = new object();
		private readonly ILog logger = LogManager.GetLogger(typeof(InProcessMessageSender));

		public InProcessMessageSender(IMessageBrokerServer messageBrokerServer)
		{
			_messageBrokerServer = messageBrokerServer;
			_actions = new ConcurrentQueue<Action>();
		}

		public void Send(Message message)
		{
			_actions.Enqueue(() => _messageBrokerServer.NotifyClients(message));
			startProcessing();
		}

		public void SendMultiple(IEnumerable<Message> messages)
		{
			_actions.Enqueue(() => _messageBrokerServer.NotifyClientsMultiple(messages));
			startProcessing();
		}

		private void startProcessing()
		{
			lock (processingLock)
			{
				if (!isProcessingQueue)
				{
					isProcessingQueue = true;
					Task.Run(() => processQueue());
				}
			}
		}

		private void processQueue()
		{
			try
			{
				while (!_actions.IsEmpty)
				{
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
			finally
			{
				isProcessingQueue = false;
			}
		}
	}
}