using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;

namespace Teleopti.Messaging.Client.Http
{
	public class HttpThrottledSender : IMessageSender, IDisposable
	{
		private readonly HttpClientM _client;
		private readonly ITime _time;
		private readonly ConcurrentQueue<Message> _queue = new ConcurrentQueue<Message>();
		private IDisposable _timer;
		private readonly TimeSpan _sendInterval;
		private readonly int _messagesPerRequest;
		private readonly object _timerLock = new object();

		public HttpThrottledSender(HttpClientM client, ITime time, IConfigReader config)
		{
			_client = client;
			_time = time;
			_sendInterval = TimeSpan.FromMilliseconds(config.ReadValue("MessageBrokerHttpSenderIntervalMilliseconds", 1000));
			_messagesPerRequest = config.ReadValue("MessageBrokerHttpSenderMessagesPerRequest", 80);
		}

		public void Send(Message message) => SendMultiple(message.AsArray());

		public void SendMultiple(IEnumerable<Message> messages)
		{
			ensureThrottle();
			messages.ForEach(m => _queue.Enqueue(m));
		}

		private void ensureThrottle()
		{
			if (_timer == null)
				_timer = _time.StartTimerWithLock(sendFromQueue, _timerLock, _sendInterval);
		}

		private void sendFromQueue()
		{
			var messages = dequeueChunk(_queue, _messagesPerRequest).ToArray();
			if (messages.Any())
				_client.Post("MessageBroker/NotifyClientsMultiple", messages);
		}

		private static IEnumerable<T> dequeueChunk<T>(ConcurrentQueue<T> queue, int chunkSize)
		{
			for (var i = 0; i < chunkSize && queue.Count > 0; i++)
			{
				queue.TryDequeue(out var message);
				yield return message;
			}
		}

		public void Dispose() =>
			_timer?.Dispose();
	}
}