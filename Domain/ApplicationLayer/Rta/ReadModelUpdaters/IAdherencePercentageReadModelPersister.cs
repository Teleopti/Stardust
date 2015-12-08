using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public interface IAdherencePercentageReadModelReader
	{
		AdherencePercentageReadModel Read(DateOnly date, Guid personId);
	}

	public interface IAdherencePercentageReadModelPersister
	{
		void Persist(AdherencePercentageReadModel model);
		AdherencePercentageReadModel Get(DateOnly date, Guid personId);
		bool HasData();
		void Delete(Guid personId);
	}
}