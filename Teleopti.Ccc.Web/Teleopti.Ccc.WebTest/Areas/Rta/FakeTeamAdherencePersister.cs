using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	public class FakeTeamAdherencePersister : ITeamAdherencePersister
	{
		private readonly List<TeamAdherenceReadModel> _models = new List<TeamAdherenceReadModel>();

		public void Persist(TeamAdherenceReadModel model)
		{
			var existing = _models.FirstOrDefault(m => m.TeamId == model.TeamId);
			if (existing != null)
			{
				existing.AgentsOutOfAdherence = model.AgentsOutOfAdherence;
			}
			else _models.Add(model);
		}

		public TeamAdherenceReadModel Get(Guid teamId)
		{
			return _models.FirstOrDefault(m => m.TeamId == teamId);
		}

		public IEnumerable<TeamAdherenceReadModel> GetForSite(Guid siteId)
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
	}
}
