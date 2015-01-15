using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IAdherenceDetailsReadModelPersister
	{
		void Add(AdherenceDetailsReadModel model);
		void Update(AdherenceDetailsReadModel model);
		AdherenceDetailsReadModel Get(Guid personId, DateOnly date);
		void ClearDetails(AdherenceDetailsReadModel model);
		bool HasData();
	}
}