using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeHistoricalAdherenceReadModelPersister:
		IHistoricalAdherenceReadModelPersister,
		IHistoricalAdherenceReadModelReader

	{
		private readonly IList<HistoricalAdherenceReadModel> _data = new List<HistoricalAdherenceReadModel>();

		public FakeHistoricalAdherenceReadModelPersister Has(HistoricalAdherenceReadModel model)
		{
			Persist(model);
			return this;
		}

		public void Persist(HistoricalAdherenceReadModel model)
		{
			_data.Add(model);
		}

		public void SetInAdherence(Guid personId)
		{
			throw new NotImplementedException();
		}

		public void SetNeutralAdherence(Guid personId)
		{
			throw new NotImplementedException();
		}

		public void SetOutOfAdherence(Guid personId)
		{
			throw new NotImplementedException();
		}

		public void UpdateSchedule(Guid personId)
		{
			throw new NotImplementedException();
		}

		public HistoricalAdherenceReadModel Get(Guid personId, DateOnly date)
		{
			return _data.SingleOrDefault(x => x.PersonId == personId && x.Date == date);
		}
	}
}