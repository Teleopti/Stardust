using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Wfm.Adherence.Historical.Infrastructure;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeHistoricalOverviewReadModelPersister : IHistoricalOverviewReadModelPersister, IHistoricalOverviewReadModelReader
	{
		private IEnumerable<HistoricalOverviewReadModel> _data = Enumerable.Empty<HistoricalOverviewReadModel>();

		public void Upsert(HistoricalOverviewReadModel model)
		{
			var existing = _data.Where(x => x.Date == model.Date && x.PersonId == model.PersonId);
			_data = _data.Except(existing).Append(model).ToArray();
		}

		public void Remove(DateTime removeUntil) => throw new NotImplementedException();

		public IEnumerable<HistoricalOverviewReadModel> Read(IEnumerable<Guid> personIds)
		{
			return personIds.IsNullOrEmpty() ? _data : _data.Where(d => personIds.Contains(d.PersonId));
		}
	}
}