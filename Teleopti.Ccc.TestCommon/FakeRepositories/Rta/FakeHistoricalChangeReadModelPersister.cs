using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeHistoricalChangeReadModelPersister : IHistoricalChangeReadModelPersister, IHistoricalChangeReadModelReader
	{
		private IEnumerable<HistoricalChangeReadModel> data = new HistoricalChangeReadModel[] {};

		public void Persist(HistoricalChangeReadModel model)
		{
			data = data.Append(model.CopyBySerialization());
		}

		public IEnumerable<HistoricalChangeReadModel> Read(Guid personId, DateOnly date)
		{
			var d = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
			return Read(personId, d, d.AddDays(1));
		}

		public IEnumerable<HistoricalChangeReadModel> Read(Guid personId, DateTime startTime, DateTime endTime)
		{
			return data
				.Where(x => x.PersonId == personId && x.Timestamp >= startTime && x.Timestamp <= endTime)
				.ToArray();
		}

		public void Remove(DateTime until)
		{
		}
	}
}