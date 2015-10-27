using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeAdherencePercentageReadModelReader : IAdherencePercentageReadModelReader
	{
		private readonly IList<AdherencePercentageReadModel> _models = new List<AdherencePercentageReadModel>();

		public FakeAdherencePercentageReadModelReader()
		{
			
		}

		public FakeAdherencePercentageReadModelReader(AdherencePercentageReadModel model)
		{
			_models.Add(model);
		}

		public FakeAdherencePercentageReadModelReader(IList<AdherencePercentageReadModel> models)
		{
			_models = models;
		}

		public AdherencePercentageReadModel Read(DateOnly date, Guid personId)
		{
			return _models.FirstOrDefault(x => x.BelongsToDate == date && x.PersonId.Equals(personId));
		}

		public void Has(AdherencePercentageReadModel adherencePercentageReadModel)
		{
			_models.Add(adherencePercentageReadModel);
		}
	}
}