using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeHistoricalChangeReadModelPersister : IHistoricalChangeReadModelPersister, IHistoricalChangeReadModelReader
	{
		private IEnumerable<HistoricalChangeReadModel> data = new HistoricalChangeReadModel[] {};

		public void Persist(HistoricalChangeReadModel model)
		{
			data = data.Append(model.CopyBySerialization());
		}

		public IEnumerable<HistoricalChangeReadModel> Read(Guid personId, DateTime date)
		{
			return data
				.Where(x => x.PersonId == personId && x.Timestamp.Date == date.Date)
				.ToArray();
		}
	}
}