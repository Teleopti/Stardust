using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class HistoricalAdherenceMaintainer :
		IHandleEvent<TenantDayTickEvent>,
		IRunOnHangfire
	{
		private readonly IHistoricalAdherenceReadModelPersister _historicalAdherencePersister;
		private readonly IHistoricalChangeReadModelPersister _historicalChangePersister;
		private readonly INow _now;

		public HistoricalAdherenceMaintainer(
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