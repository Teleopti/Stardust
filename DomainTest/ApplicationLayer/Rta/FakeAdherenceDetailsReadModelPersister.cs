using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	public class FakeAdherenceDetailsReadModelPersister : IAdherenceDetailsReadModelPersister
	{
		public IList<AdherenceDetailsReadModel> Rows = new List<AdherenceDetailsReadModel>();
		public AdherenceDetailsReadModel PersistedModel { get; set; }

		public void Add(AdherenceDetailsReadModel model)
		{
			Rows.Add(model);
			Rows = Rows.OrderBy(x => x.StartTime).ToList();
		}

		public void Update(AdherenceDetailsReadModel model)
		{
			var existing = from m in Rows
				where m.PersonId == model.PersonId &&
				      m.StartTime == model.StartTime
				select m;
			Rows.Remove(existing.Single());
			Rows.Add(model);
			Rows = Rows.OrderBy(x => x.StartTime).ToList();
		}

		public void Remove(Guid personId, DateOnly date)
		{
			var existing = from m in Rows
				where m.PersonId == personId &&
				      m.Date == date
				select m;
			Rows.Remove(existing.Single());
		}

		public IEnumerable<AdherenceDetailsReadModel> Get(Guid personId, DateOnly date)
		{
			return from m in Rows
				where m.PersonId == personId &&
				      m.Date == date
				select new AdherenceDetailsReadModel
				{
					PersonId = m.PersonId,
					Date = m.Date,
					StartTime = m.StartTime,
					Name = m.Name,
					ActualStartTime = m.ActualStartTime,
					LastStateChangedTime = m.LastStateChangedTime,
					IsInAdherence = m.IsInAdherence,
					TimeInAdherence = m.TimeInAdherence,
					TimeOutOfAdherence = m.TimeOutOfAdherence
				};
		}
	}
}