using System;
using Teleopti.Ccc.Domain.Rta.AgentAdherenceDay;
using Teleopti.Ccc.Domain.Rta.ReadModelUpdaters;

namespace Teleopti.Ccc.Domain.Rta.AdherenceChange
{
	public class AdherenceChange
	{
		private readonly IHistoricalChangeReadModelPersister _historicalChangePersister;

		public AdherenceChange(IHistoricalChangeReadModelPersister historicalChangePersister)
		{
			_historicalChangePersister = historicalChangePersister;
		}

		public void Out(Guid personId, DateTime time)
			=> _historicalChangePersister.Persist(new HistoricalChange
			{
				PersonId = personId,
				Timestamp = time,
				Adherence = HistoricalChangeAdherence.Out
			});

		public void In(Guid personId, DateTime time)
			=> _historicalChangePersister.Persist(new HistoricalChange
			{
				PersonId = personId,
				Timestamp = time,
				Adherence = HistoricalChangeAdherence.In
			});

		public void Neutral(Guid personId, DateTime time)
			=> _historicalChangePersister.Persist(new HistoricalChange
			{
				PersonId = personId,
				Timestamp = time,
				Adherence = HistoricalChangeAdherence.Neutral
			});

		public void Change(HistoricalChange change) =>
			_historicalChangePersister.Persist(change);
	}
}