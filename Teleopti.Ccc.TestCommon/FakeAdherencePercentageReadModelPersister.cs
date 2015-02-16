using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeAdherencePercentageReadModelPersister : IAdherencePercentageReadModelPersister
	{
		public FakeAdherencePercentageReadModelPersister()
		{
			PersistedModels = new List<AdherencePercentageReadModel>();
		}

		public FakeAdherencePercentageReadModelPersister(AdherencePercentageReadModel model) : this()
		{
			Persist(model);
		}
		
		public FakeAdherencePercentageReadModelPersister(IEnumerable<AdherencePercentageReadModel> models) : this()
		{
			models.ForEach(Persist);
		}

		public void Persist(AdherencePercentageReadModel model)
		{
			PersistedModels.Add(model);
		}

		public AdherencePercentageReadModel Get(DateOnly date, Guid personId)
		{
			return PersistedModels == null ? null : PersistedModels.FirstOrDefault(x => x.BelongsToDate == date && x.PersonId.Equals(personId));
		}

		public bool HasData()
		{
			return PersistedModel != null;
		}

		public AdherencePercentageReadModel PersistedModel { get { return PersistedModels.FirstOrDefault(); }}
		public IList<AdherencePercentageReadModel> PersistedModels { get; private set; }

		public void Clear()
		{
			PersistedModels.Clear();
		}
	}
}