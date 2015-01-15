using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class GetSiteAdherence : IGetSiteAdherence
	{
		private readonly ISiteOutOfAdherenceReadModelPersister _siteOutOfAdherenceReadModelPersister;

		public GetSiteAdherence(ISiteOutOfAdherenceReadModelPersister siteOutOfAdherenceReadModelPersister)
		{
			_siteOutOfAdherenceReadModelPersister = siteOutOfAdherenceReadModelPersister;
		}

		public IEnumerable<SiteOutOfAdherence> ReadAdherenceForAllSites(Guid businessUnitId)
		{
			return
				_siteOutOfAdherenceReadModelPersister.GetForBusinessUnit(businessUnitId)
					.Select(x => new SiteOutOfAdherence {Id = x.SiteId.ToString(), OutOfAdherence = x.Count});
		}
	}
}