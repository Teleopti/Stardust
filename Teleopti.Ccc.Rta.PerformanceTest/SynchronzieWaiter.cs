using System;
using Polly;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay;
using Teleopti.Wfm.Adherence.Domain.Events;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	public class SynchronzieWaiter
	{
		private readonly IKeyValueStorePersister _keyValueStore;
		private readonly IRtaEventStoreTester _events;

		public SynchronzieWaiter(IKeyValueStorePersister keyValueStore, IRtaEventStoreTester events)
		{
			_keyValueStore = keyValueStore;
			_events = events;
		}

		[TestLog]
		[UnitOfWork]
		[ReadModelUnitOfWork]
		public virtual void WaitForSyncronize(TimeSpan timeout)
		{
			var interval = TimeSpan.FromMilliseconds(100);
			var attempts = (int) (timeout.TotalMilliseconds / interval.TotalMilliseconds);
			var synchronized = Policy.HandleResult(false)
				.WaitAndRetry(attempts, attempt => interval)
				.Execute(() => SynchronizedEventId() >= LatestEventId());
			if (!synchronized)
				throw new WaitForSyncronizeException($"Events still not syncronized after waiting {timeout.TotalSeconds} seconds");
		}

		[TestLog]
		protected virtual int SynchronizedEventId() => _keyValueStore.Get(RtaEventStoreSynchronizer.SynchronizedEventKey, 0);

		[TestLog]
		protected virtual int LatestEventId() => _events.LoadLastIdForTest();

		public class WaitForSyncronizeException : Exception
		{
			public WaitForSyncronizeException(string message) : base(message)
			{
			}
		}
	}
}