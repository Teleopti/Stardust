using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Rta.AgentAdherenceDay;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeHistoricalChangeReadModelPersister : IHistoricalChangeReadModelPersister, IHistoricalChangeReadModelReader
	{
		private IEnumerable<HistoricalChange> data = new HistoricalChange[] {};

		public void Persist(HistoricalChange model)
		{
			data = data.Append(model.CopyBySerialization());
		}

		public IEnumerable<HistoricalChange> Read(Guid personId, DateOnly date)
		{
			var d = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
			return Read(personId, d, d.AddDays(1));
		}

		public IEnumerable<HistoricalChange> Read(Guid personId, DateTime startTime, DateTime endTime)
		{
			return data
				.Where(x => x.PersonId == personId && x.Timestamp >= startTime && x.Timestamp <= endTime)
				.ToArray();
		}

		public HistoricalChange ReadLastBefore(Guid personId, DateTime timestamp)
		{
			return data
				.OrderBy(x => x.Timestamp)
				.LastOrDefault(x => x.PersonId == personId && x.Timestamp < timestamp);
		}

		public void Remove(DateTime until)
		{
		}
	}
}