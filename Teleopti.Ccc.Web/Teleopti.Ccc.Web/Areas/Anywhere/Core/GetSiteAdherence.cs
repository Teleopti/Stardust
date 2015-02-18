using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class GetSiteAdherence : IGetSiteAdherence
	{
		private readonly ISiteOutOfAdherenceReadModelReader _siteOutOfAdherenceReadModelPersister;

		public GetSiteAdherence(ISiteOutOfAdherenceReadModelReader siteOutOfAdherenceReadModelPersister)
		{
			_siteOutOfAdherenceReadModelPersister = siteOutOfAdherenceReadModelPersister;
		}

		public IEnumerable<SiteOutOfAdherence> ReadAdherenceForAllSites(Guid businessUnitId)
		{
			return
				_siteOutOfAdherenceReadModelPersister.Read(businessUnitId)
					.Select(x => new SiteOutOfAdherence {Id = x.SiteId.ToString(), OutOfAdherence = x.Count});
		}
	}
}