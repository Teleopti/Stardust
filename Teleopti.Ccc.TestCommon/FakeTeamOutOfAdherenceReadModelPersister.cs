using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeTeamOutOfAdherenceReadModelPersister : ITeamOutOfAdherenceReadModelPersister, ITeamOutOfAdherenceReadModelReader
	{
		private readonly List<TeamOutOfAdherenceReadModel> _models = new List<TeamOutOfAdherenceReadModel>();

		public void Persist(TeamOutOfAdherenceReadModel model)
		{
			_models.RemoveAll(x => x.TeamId == model.TeamId);
			_models.Add(model);
		}

		public TeamOutOfAdherenceReadModel Get(Guid teamId)
		{
			return _models.FirstOrDefault(m => m.TeamId == teamId);
		}

		public IEnumerable<TeamOutOfAdherenceReadModel> Read(Guid siteId)
		{
			return _models.Where(x => x.SiteId == siteId);
		}

		public bool HasData()
		{
			return _models.Any();
		}
		
		public void Clear()
		{
			_models.Clear();
		}

		public void Has(TeamOutOfAdherenceReadModel model)
		{
			_models.Add(model);
		}
	}
}
