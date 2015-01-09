using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	public class FakeSiteAdherencePersister : ISiteAdherencePersister
	{
		private readonly List<SiteAdherenceReadModel> _models = new List<SiteAdherenceReadModel>();

		public void Persist(SiteAdherenceReadModel model)
		{
			var existing = _models.FirstOrDefault(m => m.SiteId == model.SiteId);
			if (existing != null)
			{
				existing.AgentsOutOfAdherence = model.AgentsOutOfAdherence;
			}
			else _models.Add(model);
		}

		public SiteAdherenceReadModel Get(Guid siteId)
		{
			return _models.FirstOrDefault(m => m.SiteId == siteId);
		}

		public IEnumerable<SiteAdherenceReadModel> GetAll(Guid businessUnitId)
		{
			return _models.Where(m => m.BusinessUnitId == businessUnitId);
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool HasData()
		{
			throw new NotImplementedException();
		}
	}
}