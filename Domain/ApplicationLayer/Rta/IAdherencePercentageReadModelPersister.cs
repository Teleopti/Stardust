using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IAdherencePercentageReadModelPersister
	{
		void Persist(AdherencePercentageReadModel model);
		AdherencePercentageReadModel Get(DateTime dateTime, Guid personId);
		bool HasData();
	}
}