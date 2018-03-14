using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.ApprovePeriodAsInAdherence;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels
{
	[EnabledBy(Toggles.RTA_ViewHistoricalAhderence7DaysBack_46826)]
	public class HistoricalChangeMaintainer :
		IHandleEvent<TenantDayTickEvent>,
		IRunOnHangfire
	{
		private readonly IHistoricalChangeReadModelPersister _historicalChangePersister;
		private readonly IApprovedPeriodsPersister _approvedPeriodsPersister;
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

		public HistoricalChangeMaintainer(
			IHistoricalChangeReadModelPersister historicalChangePersister,
			IApprovedPeriodsPersister approvedPeriodsPersister,
			IRtaEventStore eventStore,
			INow now,
			IConfigReader config)
		{
			_historicalChangePersister = historicalChangePersister;
			_approvedPeriodsPersister = approvedPeriodsPersister;
			_eventStore = eventStore;
			_now = now;
			_keepDays = keepDays(config);
		}

		public void Handle(TenantDayTickEvent tenantDayTickEvent)
		{
			var removeUntil = _now.UtcDateTime().Date.AddDays(_keepDays * -1);
			purgeHistoricalChange(removeUntil);
			purgeApprovedPeriods(removeUntil);
			purgeEventStore(removeUntil);
		}

		[UnitOfWork]
		protected virtual void purgeEventStore(DateTime removeUntil)
		{
			_eventStore.Remove(removeUntil);
		}

		[ReadModelUnitOfWork]
		protected virtual void purgeHistoricalChange(DateTime removeUntil)
		{
			_historicalChangePersister.Remove(removeUntil);
		}

		[UnitOfWork]
		protected virtual void purgeApprovedPeriods(DateTime removeUntil)
		{
			_approvedPeriodsPersister.Remove(removeUntil);
		}
	}

	[DisabledBy(Toggles.RTA_ViewHistoricalAhderence7DaysBack_46826)]
	public class HistoricalAdherenceMaintainerLegacy :
		IHandleEvent<TenantDayTickEvent>,
		IRunOnHangfire
	{
		private readonly IHistoricalChangeReadModelPersister _historicalChangePersister;
		private readonly INow _now;

		public HistoricalAdherenceMaintainerLegacy(
			IHistoricalChangeReadModelPersister historicalChangePersister,
			INow now)
		{
			_historicalChangePersister = historicalChangePersister;
			_now = now;
		}

		public void Handle(TenantDayTickEvent tenantDayTickEvent)
		{
			purgeHistoricalChange();
		}

		[ReadModelUnitOfWork]
		protected virtual void purgeHistoricalChange()
		{
			_historicalChangePersister.Remove(_now.UtcDateTime().Date.AddDays(-5));
		}
	}
}