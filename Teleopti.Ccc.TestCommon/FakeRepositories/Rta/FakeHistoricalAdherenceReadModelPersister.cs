using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeHistoricalAdherenceReadModelPersister:
		IHistoricalAdherenceReadModelPersister,
		IHistoricalAdherenceReadModelReader

	{
		private readonly IList<HistoricalAdherenceInternalModel> _data = new List<HistoricalAdherenceInternalModel>();

		public FakeHistoricalAdherenceReadModelPersister Has(HistoricalAdherenceReadModel model)
		{
			model.OutOfAdherences?
				.ForEach(x =>
				{
					AddOut(model.PersonId, x.StartTime);
					if (x.EndTime.HasValue)
						AddIn(model.PersonId, x.EndTime.Value);
				});
			return this;
		}

		public void AddIn(Guid personId, DateTime timestamp)
		{
			_data.Add(new HistoricalAdherenceInternalModel
			{
				PersonId = personId,
				Timestamp = timestamp,
				Adherence = 0
			});
		}

		public void AddNeutral(Guid personId, DateTime timestamp)
		{
			_data.Add(new HistoricalAdherenceInternalModel
			{
				PersonId = personId,
				Timestamp = timestamp,
				Adherence = 1
			});
		}

		public void AddOut(Guid personId, DateTime timestamp)
		{
			_data.Add(new HistoricalAdherenceInternalModel
			{
				PersonId = personId,
				Timestamp = timestamp,
				Adherence = 2
			});
		}

		public HistoricalAdherenceReadModel Read(Guid personId, DateOnly date)
		{
			var d = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
			return Read(personId, d, d.AddDays(1));
		}

		public HistoricalAdherenceReadModel Read(Guid personId, DateTime startTime, DateTime endTime)
		{
			var period = new DateTimePeriod(startTime.AddHours(-2), endTime.AddDays(1));
			var filteredData = _data
				.Where(x => x.PersonId == personId && period.Contains(x.Timestamp))
				.OrderBy(x => x.Timestamp)
				.ToArray();
			return HistoricalAdherenceReadModelReader.BuildReadModel(filteredData, personId);
		}
	}
}