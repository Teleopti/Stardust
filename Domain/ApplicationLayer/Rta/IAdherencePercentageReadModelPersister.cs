using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IAdherencePercentageReadModelPersister
	{
		void Persist(AdherencePercentageReadModel model);
		AdherencePercentageReadModel Get(DateTime dateTime, Guid personId);
		AdherencePercentageReadModel Get(DateOnly date, Guid personId);
		bool HasData();
	}
}