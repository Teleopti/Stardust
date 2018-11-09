using System;
using Polly;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Wfm.Adherence.Domain.Events
{
	public class RtaEventStoreSynchronizerWaiter
	{
		private readonly IKeyValueStorePersister _keyValueStore;
		private readonly IRtaEventStoreReader _events;
		private readonly IRtaEventStoreSynchronizer _synchronizer;
		private readonly ActiveTenants _tenants;

		public RtaEventStoreSynchronizerWaiter(
			IKeyValueStorePersister keyValueStore,
			IRtaEventStoreReader events,
			IRtaEventStoreSynchronizer synchronizer,
			ActiveTenants tenants)
		{
			_keyValueStore = keyValueStore;
			_events = events;
			_synchronizer = synchronizer;
			_tenants = tenants;
		}

		[TestLog]
		public virtual void WaitForSynchronizeAllTenants(TimeSpan timeout) =>
			_tenants.Tenants().ForEach(x => { WaitForSynchronizeTenant(x, timeout); });

		[TestLog]
		[TenantScope]
		protected virtual void WaitForSynchronizeTenant(string tenant, TimeSpan timeout) =>
			WaitForSynchronize(timeout);

		[TestLog]
		public virtual void WaitForSynchronize(TimeSpan timeout)
		{
			_synchronizer.Trigger();
			Wait(timeout);
		}

		[TestLog]
		[UnitOfWork]
		[ReadModelUnitOfWork]
		protected virtual void Wait(TimeSpan timeout)
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