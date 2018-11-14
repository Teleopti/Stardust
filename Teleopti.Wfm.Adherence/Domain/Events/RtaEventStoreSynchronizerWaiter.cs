using System;
using Polly;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Wfm.Adherence.Domain.Events
{
	public interface IRtaEventStoreSynchronizerWaiter
	{
		void Wait(TimeSpan timeout);
	}

	public class RtaEventStoreSynchronizerWaiter : IRtaEventStoreSynchronizerWaiter
	{
		private readonly IKeyValueStorePersister _keyValueStore;
		private readonly IRtaEventStoreReader _events;

		public RtaEventStoreSynchronizerWaiter(
			IKeyValueStorePersister keyValueStore,
			IRtaEventStoreReader events)
		{
			_keyValueStore = keyValueStore;
			_events = events;
		}

		[TestLog]
		[UnitOfWork]
		[ReadModelUnitOfWork]
		public virtual void Wait(TimeSpan timeout)
		{
			var interval = TimeSpan.FromMilliseconds(100);
			var attempts = (int) (timeout.TotalMilliseconds / interval.TotalMilliseconds);
			var synchronized = Policy.HandleResult(false)
				.WaitAndRetry(attempts, attempt => interval)
				.Execute(() => SynchronizedEventId() >= LatestEventId());
			if (!synchronized)
				throw new WaitForSynchronizeException($"Events still not synchronized after waiting {timeout.TotalSeconds} seconds");
		}

		[TestLog]
		protected virtual int SynchronizedEventId() =>
			Ccc.Domain.ApplicationLayer.KeyValueStorePersisterExtensions.Get(_keyValueStore, RtaEventStoreSynchronizer.SynchronizedEventKey, 0);

		[TestLog]
		protected virtual int LatestEventId() =>
			_events.ReadLastId();

		public class WaitForSynchronizeException : Exception
		{
			public WaitForSynchronizeException(string message) : base(message)
			{
			}
		}
	}
}