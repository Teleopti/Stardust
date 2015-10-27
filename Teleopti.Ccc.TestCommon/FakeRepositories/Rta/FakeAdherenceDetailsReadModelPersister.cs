using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeAdherenceDetailsReadModelPersister : IAdherenceDetailsReadModelPersister, IAdherenceDetailsReadModelReader
	{
		private readonly IList<AdherenceDetailsReadModel> _data = new List<AdherenceDetailsReadModel>();

		public FakeAdherenceDetailsReadModelPersister()
		{
		}

		public FakeAdherenceDetailsReadModelPersister(IList<AdherenceDetailsReadModel> data)
		{
			_data = data;
		}

		public AdherenceDetailsModel Model
		{
			get { return _data.Single().Model; }
		}

		public AdherenceDetailsReadModel ReadModel
		{
			get { return _data.Single(); }
		}

		public IEnumerable<ActivityAdherence> Details
		{
			get { return _data.Single().Model.Activities; }
		}

		public void Add(AdherenceDetailsReadModel model)
		{
			_data.Add(model);
		}

		public void Update(AdherenceDetailsReadModel model)
		{
			var existing = from m in _data
				where m.PersonId == model.PersonId &&
				      m.Date == model.Date
				select m;
			_data.Remove(existing.Single());
			_data.Add(model);
		}

		public bool HasData()
		{
			return _data.Any();
		}

		public AdherenceDetailsReadModel Get(Guid personId, DateOnly date)
		{
			return _data.Where(r => r.PersonId == personId && r.Date == date.Date)
				.Select(m => JsonConvert.DeserializeObject<AdherenceDetailsReadModel>(JsonConvert.SerializeObject(m)))
				.FirstOrDefault();
		}

		public AdherenceDetailsReadModel Read(Guid personId, DateOnly date)
		{
			return Get(personId, date);
		}
	}
}