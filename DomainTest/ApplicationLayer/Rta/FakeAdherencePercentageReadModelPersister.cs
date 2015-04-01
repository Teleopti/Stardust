using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	public class FakeAdherencePercentageReadModelPersister : IAdherencePercentageReadModelPersister, IAdherencePercentageReadModelReader
	{
		private readonly IList<AdherencePercentageReadModel> _data = new List<AdherencePercentageReadModel>();

		public AdherencePercentageReadModel PersistedModel { get { return PersistedModels.FirstOrDefault(); } }

		public IEnumerable<AdherencePercentageReadModel> PersistedModels { get { return _data; }}

		public FakeAdherencePercentageReadModelPersister()
		{
		}

		public FakeAdherencePercentageReadModelPersister(AdherencePercentageReadModel model)
		{
			_data.Add(model);
		}
		
		public FakeAdherencePercentageReadModelPersister(IEnumerable<AdherencePercentageReadModel> models)
		{
			models.ForEach(x => _data.Add(x));
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

		public void Clear()
		{
			_data.Clear();
		}

		public AdherencePercentageReadModel Read(DateOnly date, Guid personId)
		{
			return Get(date, personId);
		}
	}
}