using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeAdherencePercentageReadModelPersister : IAdherencePercentageReadModelPersister
	{
		public FakeAdherencePercentageReadModelPersister()
		{
		}

		public FakeAdherencePercentageReadModelPersister(AdherencePercentageReadModel model)
		{
			Persist(model);
		}

		public void Persist(AdherencePercentageReadModel model)
		{
			PersistedModel = model;
		}

		public AdherencePercentageReadModel Get(DateOnly date, Guid personId)
		{
			if (PersistedModel == null)
				return null;
			if (PersistedModel.BelongsToDate == date && PersistedModel.PersonId.Equals(personId))
				return PersistedModel;
			return null;
		}

		public bool HasData()
		{
			return PersistedModel != null;
		}

		public AdherencePercentageReadModel PersistedModel { get; private set; }

		public void Clear()
		{
			PersistedModel = null;
		}
	}
}