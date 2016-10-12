using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IHistoricalAdherenceReadModelPersister
	{
		void Persist(HistoricalAdherenceReadModel model);
		void SetInAdherence(Guid personId);
		void SetNeutralAdherence(Guid personId);
		void SetOutOfAdherence(Guid personId);
		void UpdateSchedule(Guid personId);
	}
}