using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IAdherenceDetailsReadModelReader
	{
		AdherenceDetailsReadModel Read(Guid personId, DateOnly date);
	}

	public interface IAdherenceDetailsReadModelPersister
	{
		void Add(AdherenceDetailsReadModel model);
		void Update(AdherenceDetailsReadModel model);
		AdherenceDetailsReadModel Get(Guid personId, DateOnly date);
		bool HasData();
	}
}