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

		public AdherencePercentageReadModel Get(DateTime dateTime, Guid personId)
		{
			if (!PersistedModels.Any())
				return null;
			var models = PersistedModels.Where(x => dateTime.Date >= x.BelongsToDate
				&& dateTime.Date <= x.BelongsToDate.AddDays(1)
				&& personId.Equals(x.PersonId))
				.ToList();
			foreach (var model in models.OrderBy(x => x.ShiftEndTime))
			{
				if (dateTime <= model.ShiftEndTime)
					return model;
				if (model.BelongsToDate.Date.Equals(dateTime.Date))
					return model;
			}
			return null;
		}

		public bool HasData()
		{
			return PersistedModel != null;
		}

		public AdherencePercentageReadModel PersistedModel { get { return PersistedModels.First(); }}
		public IList<AdherencePercentageReadModel> PersistedModels { get; private set; }

		public void Clear()
		{
			PersistedModels.Clear();
		}
	}
}