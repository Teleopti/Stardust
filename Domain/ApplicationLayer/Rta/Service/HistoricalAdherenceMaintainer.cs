using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class HistoricalAdherenceMaintainer :
		IHandleEvent<TenantDayTickEvent>,
		IRunOnHangfire
	{
		private readonly IHistoricalAdherenceReadModelPersister _adherencePersister;
		private readonly IHistoricalChangeReadModelPersister _historicalChangePersister;
		private readonly INow _now;

		public HistoricalAdherenceMaintainer(
			IHistoricalAdherenceReadModelPersister adherencePersister,
			IHistoricalChangeReadModelPersister historicalChangePersister,
			INow now)
		{
			_adherencePersister = adherencePersister;
			_historicalChangePersister = historicalChangePersister;
			_now = now;
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(TenantDayTickEvent tenantDayTickEvent)
		{
			_adherencePersister.Remove(_now.UtcDateTime().Date.AddDays(-5));
			_historicalChangePersister.Remove(_now.UtcDateTime().Date.AddDays(-5));
		}

	}
}