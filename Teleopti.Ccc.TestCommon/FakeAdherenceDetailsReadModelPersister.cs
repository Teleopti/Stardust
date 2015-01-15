using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeAdherenceDetailsReadModelPersister : IAdherenceDetailsReadModelPersister
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

		public IEnumerable<AdherenceDetailModel> Details
		{
			get { return _data.Single().Model.Details; }
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

		public void ClearDetails(AdherenceDetailsReadModel model)
		{
			model.Model.Details.Clear();
			Update(model);
		}

		public bool HasData()
		{
			return _data.Any();
		}

		public AdherenceDetailsReadModel Get(Guid personId, DateOnly date)
		{
			return _data.Where(r => r.PersonId == personId && r.Date == date)
				.Select(m =>
				{
					AdherenceDetailsModel model = null;

					if (m.Model != null)
					{
						var details = new List<AdherenceDetailModel>(
							from d in m.Model.Details
							select new AdherenceDetailModel
							{
								StartTime = d.StartTime,
								Name = d.Name,
								ActualStartTime = d.ActualStartTime,
								LastStateChangedTime = d.LastStateChangedTime,
								TimeInAdherence = d.TimeInAdherence,
								TimeOutOfAdherence = d.TimeOutOfAdherence
							});
						model = new AdherenceDetailsModel
						{
							Details = details,
							ShiftEndTime = m.Model.ShiftEndTime,
							ActualEndTime = m.Model.ActualEndTime,
							HasShiftEnded = m.Model.HasShiftEnded,
							IsInAdherence = m.Model.IsInAdherence,
						};
					}

					return new AdherenceDetailsReadModel
					{
						PersonId = m.PersonId,
						Date = m.Date,
						Model = model
					};
				}).FirstOrDefault();
		}
	}
}