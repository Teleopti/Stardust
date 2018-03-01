namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay
{
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