using System;
using Polly;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Wfm.Adherence.Domain.Events;

namespace Teleopti.Ccc.Rta.PerformanceTest.Code
{
	public class SynchronizerWaiter
	{
		private readonly IKeyValueStorePersister _keyValueStore;
		private readonly IRtaEventStoreReader _events;
		private readonly StatesSender _sender;
		private readonly HangfireUtilities _hangfire;

		public SynchronizerWaiter(
			IKeyValueStorePersister keyValueStore,
			IRtaEventStoreReader events,
			StatesSender sender,
			HangfireUtilities hangfire)
		{
			_keyValueStore = keyValueStore;
			_events = events;
			_sender = sender;
			_hangfire = hangfire;
		}

		[TestLog]
		public virtual void Wait(TimeSpan timeout)
		{
			var interval = TimeSpan.FromMilliseconds(500);
			var attempts = (int) (timeout.TotalMilliseconds / interval.TotalMilliseconds);
			var synchronized = Policy
				.HandleResult(false)
				.WaitAndRetry(attempts, attempt => interval)
				.Execute(() =>
				{
					_sender.TriggerRecurringJobs();
					_hangfire.WaitForQueue();
					return SynchronizedEventId() >= LatestEventId();
				});
			if (!synchronized)
				throw new WaitForSynchronizeException($"Events still not synchronized after waiting {timeout.TotalSeconds} seconds");
		}

		[TestLog]
		[ReadModelUnitOfWork]
		protected virtual long SynchronizedEventId() =>
			_keyValueStore.Get(RtaEventStoreSynchronizer.SynchronizedEventKey, 0);

		[TestLog]
		[UnitOfWork]
		protected virtual long LatestEventId() =>
			_events.ReadLastId();

		public class WaitForSynchronizeException : Exception
		{
			public WaitForSynchronizeException(string message) : base(message)
			{
			}
		}
	}
}