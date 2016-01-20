using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeSiteOutOfAdherenceReadModelPersister : ISiteOutOfAdherenceReadModelPersister, ISiteOutOfAdherenceReadModelReader
	{
		private readonly List<SiteOutOfAdherenceReadModel> _models = new List<SiteOutOfAdherenceReadModel>();

		public void Has(SiteOutOfAdherenceReadModel model)
		{
			Persist(model);
		}

		public void Persist(SiteOutOfAdherenceReadModel model)
		{
			_models.RemoveAll(x => x.SiteId == model.SiteId);
			_models.Add(model);
		}

		public IEnumerable<SiteOutOfAdherenceReadModel> Read()
		{
			return _models.Select(m => JsonConvert.DeserializeObject<SiteOutOfAdherenceReadModel>(JsonConvert.SerializeObject(m))).ToArray();
		}

		public SiteOutOfAdherenceReadModel Get(Guid siteId)
		{
			return _models.FirstOrDefault(m => m.SiteId == siteId);
		}

		public IEnumerable<SiteOutOfAdherenceReadModel> GetAll()
		{
			return _models.ToArray();
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