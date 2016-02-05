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
		private IList<AdherenceDetailsReadModel> _data = new List<AdherenceDetailsReadModel>();

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
		
		public void Persist(AdherenceDetailsReadModel model)
		{
			var existing = _data.SingleOrDefault(m => m.PersonId == model.PersonId && m.Date == model.Date);
			if (existing != null)
				_data.Remove(existing);
			_data.Add(model);
		}

		public void Delete(Guid personId)
		{
			_data = _data.Where(x => x.PersonId != personId).ToList();
		}

		public bool HasData()
		{
			return _data.Any();
		}


		public AdherenceDetailsReadModel Get(DateOnly date, Guid personId)
		{
			return _data.Where(r => r.PersonId == personId && r.Date == date.Date)
				.Select(m => JsonConvert.DeserializeObject<AdherenceDetailsReadModel>(JsonConvert.SerializeObject(m)))
				.FirstOrDefault();
		}

		public AdherenceDetailsReadModel Read(Guid personId, DateOnly date)
		{
			return Get(date, personId);
		}
	}
}