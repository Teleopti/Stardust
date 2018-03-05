using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay
{
	[RemoveMeWithToggle(Toggles.RTA_RemoveApprovedOOA_47721)]
	public class HistoricalChange
	{
		private readonly IHistoricalChangeReadModelPersister _historicalChangePersister;

		public HistoricalChange(IHistoricalChangeReadModelPersister historicalChangePersister)
		{
			_historicalChangePersister = historicalChangePersister;
		}

		public void Change(HistoricalChangeModel change) =>
			_historicalChangePersister.Persist(change);
	}
}