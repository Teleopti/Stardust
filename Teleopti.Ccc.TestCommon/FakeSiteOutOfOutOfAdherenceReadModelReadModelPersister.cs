using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeSiteOutOfOutOfAdherenceReadModelReadModelPersister : ISiteOutOfAdherenceReadModelPersister
	{
		private readonly List<SiteOutOfAdherenceReadModel> _models = new List<SiteOutOfAdherenceReadModel>();

		public void Persist(SiteOutOfAdherenceReadModel model)
		{
			_models.RemoveAll(x => x.SiteId == model.SiteId);
			_models.Add(model);
		}

		public SiteOutOfAdherenceReadModel Get(Guid siteId)
		{
			return _models.FirstOrDefault(m => m.SiteId == siteId);
		}

		public IEnumerable<SiteOutOfAdherenceReadModel> GetForBusinessUnit(Guid businessUnitId)
		{
			return _models.Where(x => x.BusinessUnitId == businessUnitId);
		}

		public void Clear()
		{
			_models.Clear();
		}

		public bool HasData()
		{
			return _models.Any();
		}
	}
}