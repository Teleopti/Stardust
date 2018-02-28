using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ApprovePeriodAsInAdherence;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	[EnabledBy(Toggles.RTA_ViewHistoricalAhderence7DaysBack_46826)]
	public class HistoricalAdherenceMaintainer :
		IHandleEvent<TenantDayTickEvent>,
		IRunOnHangfire
	{
		private readonly IHistoricalAdherenceReadModelPersister _historicalAdherencePersister;
		private readonly IHistoricalChangeReadModelPersister _historicalChangePersister;
		private readonly IApprovedPeriodsPersister _approvedPeriodsPersister;
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

		public HistoricalAdherenceMaintainer(
			IHistoricalAdherenceReadModelPersister historicalAdherencePersister,
			IHistoricalChangeReadModelPersister historicalChangePersister,
			IApprovedPeriodsPersister approvedPeriodsPersister,
			INow now,
			IConfigReader config)
		{
			_historicalAdherencePersister = historicalAdherencePersister;
			_historicalChangePersister = historicalChangePersister;
			_approvedPeriodsPersister = approvedPeriodsPersister;
			_now = now;
			_keepDays = keepDays(config);
		}

		public void Handle(TenantDayTickEvent tenantDayTickEvent)
		{
			var removeUntil = _now.UtcDateTime().Date.AddDays(_keepDays * -1);
			purgeHistoricalAdherence(removeUntil);
			purgeHistoricalChange(removeUntil);
			purgeApprovedPeriods(removeUntil);
		}

		[ReadModelUnitOfWork]
		protected virtual void purgeHistoricalChange(DateTime removeUntil)
		{
			_historicalChangePersister.Remove(removeUntil);
		}

		[ReadModelUnitOfWork]
		protected virtual void purgeHistoricalAdherence(DateTime removeUntil)
		{
			_historicalAdherencePersister.Remove(removeUntil);
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
		private readonly IHistoricalAdherenceReadModelPersister _historicalAdherencePersister;
		private readonly IHistoricalChangeReadModelPersister _historicalChangePersister;
		private readonly INow _now;

		public HistoricalAdherenceMaintainerLegacy(
			IHistoricalAdherenceReadModelPersister historicalAdherencePersister,
			IHistoricalChangeReadModelPersister historicalChangePersister,
			INow now)
		{
			_historicalAdherencePersister = historicalAdherencePersister;
			_historicalChangePersister = historicalChangePersister;
			_now = now;
		}

		public void Handle(TenantDayTickEvent tenantDayTickEvent)
		{
			purgeHistoricalAdherence();
			purgeHistoricalChange();
		}

		[ReadModelUnitOfWork]
		protected virtual void purgeHistoricalChange()
		{
			_historicalChangePersister.Remove(_now.UtcDateTime().Date.AddDays(-5));
		}

		[ReadModelUnitOfWork]
		protected virtual void purgeHistoricalAdherence()
		{
			_historicalAdherencePersister.Remove(_now.UtcDateTime().Date.AddDays(-5));
		}
	}
}