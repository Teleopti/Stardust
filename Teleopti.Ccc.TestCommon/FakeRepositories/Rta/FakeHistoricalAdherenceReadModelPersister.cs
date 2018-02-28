using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Rta.AgentAdherenceDay;
using Teleopti.Ccc.Domain.Rta.ReadModelUpdaters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class HistoricalOutOfAdherenceReadModel
	{
		public DateTime StartTime { get; set; }
		public DateTime? EndTime { get; set; }
	}

	public class FakeHistoricalAdherenceReadModelPersister:
		IHistoricalAdherenceReadModelPersister,
		IHistoricalAdherenceReadModelReader

	{
		private IList<HistoricalAdherence> _data = new List<HistoricalAdherence>();

		public void AddIn(Guid personId, DateTime timestamp)
		{
			add(personId, timestamp, HistoricalAdherenceAdherence.In);
		}

		public void AddNeutral(Guid personId, DateTime timestamp)
		{
			add(personId, timestamp, HistoricalAdherenceAdherence.Neutral);
		}

		public void AddOut(Guid personId, DateTime timestamp)
		{
			add(personId, timestamp, HistoricalAdherenceAdherence.Out);
		}

		public void Remove(DateTime until)
		{
			_data = _data.Where(x => x.Timestamp >= until).ToArray();
		}

		private void add(Guid personId, DateTime timestamp, HistoricalAdherenceAdherence adherence)
		{
			_data.Add(new HistoricalAdherence
			{
				PersonId = personId,
				Timestamp = timestamp,
				Adherence = adherence
			});
		}

		public IEnumerable<HistoricalAdherence> Read(Guid personId, DateOnly date)
		{
			var d = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
			return Read(personId, d, d.AddDays(1));
		}

		public IEnumerable<HistoricalAdherence> Read(Guid personId, DateTime startTime, DateTime endTime)
		{
			var period = new DateTimePeriod(startTime, endTime);
			return _data
				.Where(x => x.PersonId == personId && period.Contains(x.Timestamp))
				.OrderBy(x => x.Timestamp)
				.ToArray();
		}

		public HistoricalAdherence ReadLastBefore(Guid personId, DateTime timestamp)
		{
			return _data
				.OrderBy(x => x.Timestamp)
				.LastOrDefault(x => x.PersonId == personId && x.Timestamp < timestamp);
		}

		public IEnumerable<HistoricalAdherence> Read(Guid personId)
		{
			return _data.Where(x => x.PersonId == personId).ToArray();
		}
	}
}