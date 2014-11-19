using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IAdherenceDetailsReadModelPersister
	{
		void Add(AdherenceDetailsReadModel model);
		void Update(AdherenceDetailsReadModel model);
		IEnumerable<AdherenceDetailsReadModel> Get(Guid personId, DateOnly date);
		void Remove(Guid personId, DateOnly date);
	}
}