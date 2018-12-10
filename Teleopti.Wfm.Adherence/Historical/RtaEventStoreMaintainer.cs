using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.Historical.Infrastructure;

namespace Teleopti.Wfm.Adherence.Historical
{
	public class RtaEventStoreMaintainer :
		IHandleEvent<TenantDayTickEvent>,
		IRunOnHangfire
	{
		private readonly IRtaEventStore _eventStore;
		private readonly INow _now;
		private readonly int _keepDays;

		private static int keepDays(IConfigReader config)
		{
			var keepDays = config.ReadValue("HistoricalAdherenceKeepDays", 7);
			const int weekendExtraForInitialAdherenceValueMondayMorning = 2;
			const int timeZoneDifferenceExtra = 2;
			return keepDays + weekendExtraForInitialAdherenceValueMondayMorning + timeZoneDifferenceExtra;
		}

		public static int DisplayPastDays(IConfigReader config)
		{
			return config.ReadValue("HistoricalAdherenceKeepDays", 7) - 1;
		}

		public RtaEventStoreMaintainer(
			IRtaEventStore eventStore,
			INow now,
			IConfigReader config)
		{
			_eventStore = eventStore;
			_now = now;
			_keepDays = keepDays(config);
		}

		public void Handle(TenantDayTickEvent tenantDayTickEvent)
		{
			var removeUntil = _now.UtcDateTime().Date.AddDays(_keepDays * -1);
			purgeEventStoreInBatch(removeUntil);
		}

		private void purgeEventStoreInBatch(DateTime removeUntil)
		{
			const int maxEventsToRemove = 10000;
			var deletedEventsCount = 0;

			do
			{
				deletedEventsCount = purgeEventStore(removeUntil, maxEventsToRemove);
			} while (deletedEventsCount == maxEventsToRemove);
		}

		[UnitOfWork]
		protected virtual int purgeEventStore(DateTime removeUntil, int maxEventsToRemove) =>
			_eventStore.Remove(removeUntil, maxEventsToRemove);
	}
}