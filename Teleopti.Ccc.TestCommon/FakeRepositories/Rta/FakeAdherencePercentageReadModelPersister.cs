using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeAdherencePercentageReadModelPersister : IAdherencePercentageReadModelPersister, IAdherencePercentageReadModelReader
	{
		private readonly INow _now;
		private IList<AdherencePercentageReadModel> _data = new List<AdherencePercentageReadModel>();

		public AdherencePercentageReadModel PersistedModel { get { return PersistedModels.FirstOrDefault(); } }

		public IEnumerable<AdherencePercentageReadModel> PersistedModels { get { return _data; }}

		public FakeAdherencePercentageReadModelPersister(INow now)
		{
			_now = now;
		}

		public void Has(AdherencePercentageReadModel adherencePercentageReadModel)
		{
			_data.Add(adherencePercentageReadModel);
		}

		public void Persist(AdherencePercentageReadModel model)
		{
			_data.Add(model);
		}

		public AdherencePercentageReadModel Get(DateOnly date, Guid personId)
		{
			return _data.FirstOrDefault(x => x.BelongsToDate == date && x.PersonId.Equals(personId));
		}

		public bool HasData()
		{
			return _data.Any();
		}

		public void Delete(Guid personId)
		{
			_data = _data.Where(x => x.PersonId != personId).ToList();
		}

		public void Clear()
		{
			_data.Clear();
		}

		public AdherencePercentageReadModel ReadCurrent(Guid personId)
		{
			return Get(new DateOnly(_now.UtcDateTime()), personId);
		}
	}
}