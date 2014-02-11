using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using log4net;

namespace Teleopti.Messaging.SignalR
{
	public class AsyncSignalSender : SignalSenderBase, IAsyncMessageSender
	{
		private readonly BlockingCollection<Tuple<DateTime, Notification>> _notificationQueue = new BlockingCollection<Tuple<DateTime, Notification>>();
		private readonly CancellationTokenSource cancelToken = new CancellationTokenSource();
		private Thread workerThread;

		public AsyncSignalSender(string serverUrl)
			: base(serverUrl)
		{
			Logger = MakeLogger();
			StartWorkerThread();
		}

		public void SendNotificationAsync(Notification notification)
		{
			_notificationQueue.Add(new Tuple<DateTime, Notification>(CurrentUtcTime(), notification));
		}

		private void processQueue(object state)
		{
			while (!cancelToken.IsCancellationRequested)
			{
				ProcessTheQueue();
			}
		}

		protected virtual void ProcessTheQueue()
		{
			var notifications =
				_notificationQueue.GetConsumingEnumerable(cancelToken.Token)
				                  .Take(Math.Max(1, Math.Min(_notificationQueue.Count, 20)))
				                  .ToArray()
				                  .Where(t => t.Item1 > CurrentUtcTime().AddMinutes(-2))
				                  .Select(t => t.Item2)
				                  .ToArray();

			if (!notifications.Any()) return;
			Wrapper.NotifyClients(notifications);
		}

		public override void Dispose()
		{
			cancelToken.Cancel();
			workerThread.Join();
			base.Dispose();
		}


		[CLSCompliant(false)]
		protected virtual DateTime CurrentUtcTime()
		{
			return DateTime.UtcNow;
		}

		[CLSCompliant(false)]
		protected virtual void StartWorkerThread()
		{
			workerThread = new Thread(processQueue) { IsBackground = true };
			workerThread.Start();
		}

		[CLSCompliant(false)]
		protected virtual ILog MakeLogger()
		{
			return LogManager.GetLogger(typeof(AsyncSignalSender));
		}
	}
}