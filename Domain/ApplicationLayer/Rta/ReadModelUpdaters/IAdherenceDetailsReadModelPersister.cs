using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public interface IAdherenceDetailsReadModelReader
	{
		AdherenceDetailsReadModel Read(Guid personId, DateOnly date);
	}

	public interface IAdherenceDetailsReadModelPersister
	{
		void Persist(AdherenceDetailsReadModel model);
		void Delete(Guid personId);
		AdherenceDetailsReadModel Get(DateOnly date, Guid personId);
		bool HasData();
	}
}