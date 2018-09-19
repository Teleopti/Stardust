using System.Threading;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	public class SynchronzieWaiter
	{
		private readonly WithUnitOfWork _unitOfWork;
		private readonly WithReadModelUnitOfWork _readModelUnitOfWork;
		private readonly IKeyValueStorePersister _keyValueStore;
		private readonly IRtaEventStoreTestReader _events;

		public SynchronzieWaiter(WithUnitOfWork unitOfWork, WithReadModelUnitOfWork readModelUnitOfWork, IKeyValueStorePersister keyValueStore, IRtaEventStoreTestReader events)
		{
			_unitOfWork = unitOfWork;
			_readModelUnitOfWork = readModelUnitOfWork;
			_keyValueStore = keyValueStore;
			_events = events;
		}

		[TestLog]
		public virtual void WaitForSyncronize()
		{
			while (true)
			{
				if (GetLatestStoredRtaEventId() == GetLatestSynchronizedRtaEventId())
					break;
				Thread.Sleep(100);
			}
		}

		[TestLog]
		protected virtual int GetLatestSynchronizedRtaEventId()
		{
			return _readModelUnitOfWork.Get(() => _keyValueStore.Get("LatestSynchronizedRTAEvent", 0));
		}

		[TestLog]
		protected virtual int GetLatestStoredRtaEventId()
		{
			return _unitOfWork.Get(() => _events.LoadLastIdForTest());
		}
	}
}