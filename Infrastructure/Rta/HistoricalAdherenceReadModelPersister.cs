using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class HistoricalAdherenceReadModelPersister : IHistoricalAdherenceReadModelPersister {

		private readonly ICurrentReadModelUnitOfWork _unitOfWork;
		private readonly IJsonSerializer _serializer;

		public HistoricalAdherenceReadModelPersister(ICurrentReadModelUnitOfWork unitOfWork, IJsonSerializer serializer)
		{
			_unitOfWork = unitOfWork;
			_serializer = serializer;
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