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
		}

		public void Update(AdherenceDetailsReadModel model)
		{
			var existing = from m in Rows
				where m.PersonId == model.PersonId &&
				      m.Date == model.Date
				select m;
			Rows.Remove(existing.Single());
			Rows.Add(model);
		}

		public void Remove(Guid personId, DateOnly date)
		{
			var existing = from m in Rows
				where m.PersonId == personId &&
				      m.Date == date
				select m;
			Rows.Remove(existing.Single());
		}

		public void ClearDetails(AdherenceDetailsReadModel model)
		{
			model.Model.DetailModels.Clear();
			Update(model);
		}

		public AdherenceDetailsReadModel Get(Guid personId, DateOnly date)
		{
			return Rows.Where(r => r.PersonId == personId && r.Date == date)
				.Select(m => new AdherenceDetailsReadModel
				{
					PersonId = m.PersonId,
					Date = m.Date,
					Model = new AdherenceDetailsModel
					{
						DetailModels = new List<AdherenceDetailModel>(
							from d in m.Model.DetailModels
							select new AdherenceDetailModel
							{
								StartTime = d.StartTime,
								Name = d.Name,
								ActualStartTime = d.ActualStartTime,
								LastStateChangedTime = d.LastStateChangedTime,
								IsInAdherence = d.IsInAdherence,
								TimeInAdherence = d.TimeInAdherence,
								TimeOutOfAdherence = d.TimeOutOfAdherence
							}),
						ShiftEndTime = m.Model.ShiftEndTime,
						ActualEndTime = m.Model.ActualEndTime,
						HasShiftEnded = m.Model.HasShiftEnded,
						HasActivityEnded = m.Model.HasActivityEnded
					}
				}).FirstOrDefault();
		}
	}
}