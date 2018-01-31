using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class HistoricalAdherenceReadModelPersister : IHistoricalAdherenceReadModelPersister
	{
		private readonly ICurrentReadModelUnitOfWork _unitOfWork;

		public HistoricalAdherenceReadModelPersister(ICurrentReadModelUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public void AddIn(Guid personId, DateTime timestamp)
		{
			add(personId, timestamp, HistoricalAdherenceAdherence.In);
		}

		public void AddNeutral(Guid personId, DateTime timestamp)
		{
			add(personId, timestamp, HistoricalAdherenceAdherence.Neutral);
		}

		public void AddOut(Guid personId, DateTime timestamp)
		{
			add(personId, timestamp, HistoricalAdherenceAdherence.Out);
		}

		public void Remove(DateTime until)
		{
			_unitOfWork.Current()
				.CreateSqlQuery(@"
DELETE FROM [ReadModel].[HistoricalAdherence] WHERE Timestamp < :ts")
				.SetParameter("ts", until)
				.ExecuteUpdate();
		}

		private void add(Guid personId, DateTime timestamp, HistoricalAdherenceAdherence adherence)
		{
			_unitOfWork.Current()
				.CreateSqlQuery(@"
INSERT INTO [ReadModel].[HistoricalAdherence] (PersonId, Timestamp, Adherence)
VALUES (:PersonId, :Timestamp, :Adherence)")
				.SetParameter("PersonId", personId)
				.SetParameter("Timestamp", timestamp)
				.SetParameter("Adherence", (int) adherence)
				.ExecuteUpdate();
		}

	}
}