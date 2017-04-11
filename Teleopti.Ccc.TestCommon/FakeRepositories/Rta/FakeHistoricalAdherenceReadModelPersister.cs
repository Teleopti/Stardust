using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeHistoricalAdherenceReadModelPersister:
		IHistoricalAdherenceReadModelPersister,
		IHistoricalAdherenceReadModelReader

	{
		private IList<HistoricalAdherenceReadModel> _data = new List<HistoricalAdherenceReadModel>();

		public FakeHistoricalAdherenceReadModelPersister Has(IEnumerable<HistoricalAdherenceReadModel> data)
		{
			_data = _data.Concat(data).ToArray();
			return this;
		}

		public FakeHistoricalAdherenceReadModelPersister Has(Guid personId, IEnumerable<HistoricalOutOfAdherenceReadModel> models)
		{
			models
				.ForEach(x =>
				{
					AddOut(personId, x.StartTime);
					if (x.EndTime.HasValue)
						AddIn(personId, x.EndTime.Value);
				});
			return this;
		}

		public void AddIn(Guid personId, DateTime timestamp)
		{
			add(personId, timestamp, HistoricalAdherenceReadModelAdherence.In);
		}

		public void AddNeutral(Guid personId, DateTime timestamp)
		{
			add(personId, timestamp, HistoricalAdherenceReadModelAdherence.Neutral);
		}

		public void AddOut(Guid personId, DateTime timestamp)
		{
			add(personId, timestamp, HistoricalAdherenceReadModelAdherence.Out);
		}

		public void Remove(DateTime until)
		{
			_data = _data.Where(x => x.Timestamp >= until).ToArray();
		}

		private void add(Guid personId, DateTime timestamp, HistoricalAdherenceReadModelAdherence adherence)
		{
			_data.Add(new HistoricalAdherenceReadModel
			{
				PersonId = personId,
				Timestamp = timestamp,
				Adherence = adherence
			});
		}

		public IEnumerable<HistoricalAdherenceReadModel> Read(Guid personId, DateOnly date)
		{
			var d = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
			return Read(personId, d, d.AddDays(1));
		}

		public IEnumerable<HistoricalAdherenceReadModel> Read(Guid personId, DateTime startTime, DateTime endTime)
		{
			var period = new DateTimePeriod(startTime, endTime);
			return _data
				.Where(x => x.PersonId == personId && period.Contains(x.Timestamp))
				.OrderBy(x => x.Timestamp)
				.ToArray();
		}

		public HistoricalAdherenceReadModel ReadLastBefore(Guid personId, DateTime timestamp)
		{
			return _data
				.OrderBy(x => x.Timestamp)
				.LastOrDefault(x => x.PersonId == personId && x.Timestamp < timestamp);
		}

		public IEnumerable<HistoricalAdherenceReadModel> Read(Guid personId)
		{
			return _data.Where(x => x.PersonId == personId).ToArray();
		}
	}
}