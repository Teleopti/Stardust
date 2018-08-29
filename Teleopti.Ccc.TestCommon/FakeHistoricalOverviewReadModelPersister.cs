using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeHistoricalOverviewReadModelPersister : IHistoricalOverviewReadModelPersister, IHistoricalOverviewReadModelReader
	{
		private IEnumerable<HistoricalOverviewReadModel> _data = Enumerable.Empty<HistoricalOverviewReadModel>();
		
		public void Upsert(HistoricalOverviewReadModel model)
		{
			var existing = _data.Where(x => x.Date.ToDateOnly() == model.Date.ToDateOnly() && x.PersonId == model.PersonId);
			_data = _data.Except(existing).Append(model).ToArray();
		}

		public IEnumerable<HistoricalOverviewReadModel> Read(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds)
		{
			var data = _data;
			if (!teamIds.IsNullOrEmpty())
				return data.Where(d => d.TeamId != null && teamIds.Contains(d.TeamId.Value));
			return data;
		}
	}
}