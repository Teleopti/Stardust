using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class HistoricalAdherenceReadModelPersister : IHistoricalAdherenceReadModelPersister {

		private readonly ICurrentReadModelUnitOfWork _unitOfWork;

		public HistoricalAdherenceReadModelPersister(ICurrentReadModelUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public void AddIn(Guid personId, DateTime timestamp)
		{
			add(personId, timestamp, 0);
		}

		public void AddNeutral(Guid personId, DateTime timestamp)
		{
			add(personId, timestamp, 1);
		}

		public void AddOut(Guid personId, DateTime timestamp)
		{
			add(personId, timestamp, 2);
		}

		private void add(Guid personId, DateTime timestamp, int adherence)
		{
			_unitOfWork.Current()
				.CreateSqlQuery(@"
INSERT INTO [ReadModel].[HistoricalAdherence] (PersonId, Timestamp, Adherence)
VALUES (:PersonId, :Timestamp, :Adherence)")
				.SetParameter("PersonId", personId)
				.SetParameter("Timestamp", timestamp)
				.SetParameter("Adherence", adherence)
				.ExecuteUpdate();
		}
	}

}