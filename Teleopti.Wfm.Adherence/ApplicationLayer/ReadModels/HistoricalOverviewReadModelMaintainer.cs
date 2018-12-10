using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.Historical.Infrastructure;

namespace Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels
{
	public class HistoricalOverviewReadModelMaintainer :
		IHandleEvent<TenantDayTickEvent>,
		IRunOnHangfire
	{
		private readonly IHistoricalOverviewReadModelPersister _persister;
		private readonly INow _now;
		private readonly int _keepDays;

		private static int keepDays(IConfigReader config)
		{
			var keepDays = config.ReadValue("HistoricalAdherenceKeepDays", 7);
			const int weekendExtraForInitialAdherenceValueMondayMorning = 2;
			const int timeZoneDifferenceExtra = 2;
			return keepDays + weekendExtraForInitialAdherenceValueMondayMorning + timeZoneDifferenceExtra;
		}

		public HistoricalOverviewReadModelMaintainer(
			INow now,
			IConfigReader config, 
			IHistoricalOverviewReadModelPersister persister)
		{
			_persister = persister;
			_now = now;
			_persister = persister;
			_keepDays = keepDays(config);
		}

		[UnitOfWork]
		public virtual void Handle(TenantDayTickEvent tenantDayTickEvent)
		{
			var removeUntil = _now.UtcDateTime().Date.AddDays(_keepDays * -1);
			_persister.Remove(removeUntil);
		}		
	}
}