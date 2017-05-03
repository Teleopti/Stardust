using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
			var existing = _data.SingleOrDefault(x => x.PersonId == model.PersonId);
			if (existing == null)
				_data.Add(model);
			else
			{
				_data.Remove(existing);
				_data.Add(model);
			}
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