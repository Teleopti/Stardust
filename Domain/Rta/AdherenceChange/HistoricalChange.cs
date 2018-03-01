using System;
using Teleopti.Ccc.Domain.Rta.AgentAdherenceDay;
using Teleopti.Ccc.Domain.Rta.ReadModelUpdaters;

namespace Teleopti.Ccc.Domain.Rta.AdherenceChange
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